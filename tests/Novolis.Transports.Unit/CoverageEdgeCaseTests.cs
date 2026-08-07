using System.Buffers.Binary;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Novolis.Transports.Http.Authentication.Oidc;
using Novolis.Transports.LocalIpc;
using Novolis.Transports.Tcp.Abstractions;
using Novolis.Transports.Tcp.Cryptography;

namespace Novolis.Transports.Unit;

public sealed class CoverageEdgeCaseTests
{
    [Test]
    public async Task LocalIpc_Codec_Handles_Empty_And_Truncated_Streams()
    {
        await using var empty = new MemoryStream();
        await Assert.That(await LocalIpcFrameCodec.ReadAsync(empty)).IsNull();

        await using var shortPrefix = new MemoryStream([1, 2]);
        await Assert.That(await LocalIpcFrameCodec.ReadAsync(shortPrefix)).IsNull();

        await using var shortFrame = new MemoryStream([8, 0, 0, 0, 1, 2]);
        await Assert.That(await LocalIpcFrameCodec.ReadAsync(shortFrame)).IsNull();
    }

    [Test]
    public async Task LocalIpc_Codec_Rejects_Negative_Frame_Length()
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, -1);
        await using var stream = new MemoryStream(bytes);

        await Assert.ThrowsAsync<InvalidDataException>(async () => await LocalIpcFrameCodec.ReadAsync(stream));
    }

    [Test]
    public async Task LocalIpc_Codec_Rejects_Negative_And_Truncated_Payloads()
    {
        await using var negative = BuildEncodedFrame(payloadLength: -1, body: []);
        await Assert.ThrowsAsync<InvalidDataException>(async () => await LocalIpcFrameCodec.ReadAsync(negative));

        await using var truncated = BuildEncodedFrame(payloadLength: 4, body: [1, 2]);
        await Assert.ThrowsAsync<EndOfStreamException>(async () => await LocalIpcFrameCodec.ReadAsync(truncated));
    }

    [Test]
    public async Task LocalIpc_Codec_Validates_Null_Arguments()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await LocalIpcFrameCodec.WriteAsync(null!, new LocalIpcFrame(0, "", "", [])));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await LocalIpcFrameCodec.WriteAsync(new MemoryStream(), null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await LocalIpcFrameCodec.ReadAsync(null!));
    }

    [Test]
    public async Task LocalIpc_Auto_Factories_Create_Headless_Endpoints()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var endpoint = new LocalIpcEndpoint($"novolis-coverage-{Guid.NewGuid():N}");
        await using var listener = LocalIpcTransport.CreateListener(endpoint);
        var client = LocalIpcTransport.CreateClient();

        await Assert.That(client).IsNotNull();
        await Assert.That(listener).IsNotNull();
        await Assert.That(endpoint.Kind).IsEqualTo(LocalIpcTransportKind.Auto);
        await Assert.That(endpoint.Address).StartsWith("novolis-coverage-");
    }

    [Test]
    public async Task Tcp_Pipeline_Covers_Null_Middleware_And_Validation()
    {
        TcpConnectionRequestDelegate terminal = input => ValueTask.FromResult(input);
        var payload = new byte[] { 3, 2, 1 };

        var direct = TcpConnectionPipeline.Build(terminal, null);
        var response = await direct(payload);
        var memoryResponse = await MemoryTcpTransport.RoundTripAsync(terminal, payload);

        await Assert.That(response.ToArray()).IsEquivalentTo(payload);
        await Assert.That(memoryResponse.ToArray()).IsEquivalentTo(payload);
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
        {
            TcpConnectionPipeline.Build(null!, []);
            return Task.CompletedTask;
        });
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
        {
            MemoryTcpTransport.RoundTripAsync(null!, payload);
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Tcp_Cryptography_Covers_Defaults_Factory_And_Null_Legacy_Configuration()
    {
        var options = new TcpPayloadEncryptionOptions();
        var key = options.ToAesKey();
        using var aes = new TcpPayloadEncryptorFactory().Create(key);

        await Assert.That(key.Key.Length).IsEqualTo(32);
        await Assert.That(key.Iv.Length).IsEqualTo(16);
        await Assert.That(aes.KeySize).IsEqualTo(256);
        await Assert.That(aes.BlockSize).IsEqualTo(128);

#pragma warning disable CS0618
        var services = new ServiceCollection();
        services.AddAdvancedEncryption();
#pragma warning restore CS0618
        await using var provider = services.BuildServiceProvider();
        await Assert.That(provider.GetRequiredService<ITcpPayloadEncryptor>()).IsNotNull();
    }

    [Test]
    public async Task Oidc_TokenProvider_Uses_Prepopulated_Cache()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set("oidc-token", "cached-token");
        using var http = new HttpClient(new FailingHandler());
        var options = Options.Create(new OidcAuthenticationConfiguration
        {
            ClientId = "client",
            ClientSecret = "secret",
            Scope = "scope",
            TokenEndpoint = null,
        });

        var token = await new OidcTokenProvider(http, options, cache).GetTokenAsync(CancellationToken.None);

        await Assert.That(token).IsEqualTo("cached-token");
    }

    [Test]
    public async Task Oidc_TokenProvider_Rejects_Empty_Json_Body()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var http = new HttpClient(new EmptyJsonHandler());
        var options = Options.Create(new OidcAuthenticationConfiguration
        {
            ClientId = "client",
            ClientSecret = "secret",
            Scope = "scope",
            TokenEndpoint = "https://identity.test/token",
        });

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await new OidcTokenProvider(http, options, cache).GetTokenAsync(CancellationToken.None));
    }

    private static MemoryStream BuildEncodedFrame(int payloadLength, byte[] body)
    {
        using var payload = new MemoryStream();
        using (var writer = new BinaryWriter(payload, new UTF8Encoding(false, true), leaveOpen: true))
        {
            writer.Write(1L);
            writer.Write("kind");
            writer.Write("name");
            writer.Write(payloadLength);
            writer.Write(body);
        }

        var encoded = payload.ToArray();
        var stream = new MemoryStream();
        var prefix = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(prefix, encoded.Length);
        stream.Write(prefix);
        stream.Write(encoded);
        stream.Position = 0;
        return stream;
    }

    private sealed class FailingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("HTTP should not be called for a cached token.");
    }

    private sealed class EmptyJsonHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json"),
            });
    }
}
