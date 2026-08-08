using Novolis.Transports.Http;
using Novolis.Transports.Http.Abstractions;
using Novolis.Transports.Http.Extensions;
using Novolis.Transports.Http.Tests.Infrastructure;
using Novolis.Transports.LocalIpc;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Unit.LocalIpc;

public sealed class LocalIpcUnixAndAutoTests
{
    [Test]
    public async Task UnixDomainSocket_round_trip_send_and_read()
    {
        var path = Path.Combine(Path.GetTempPath(), $"novolis-ipc-{Guid.NewGuid():N}.sock");
        try
        {
            var endpoint = new LocalIpcEndpoint(path, LocalIpcTransportKind.UnixDomainSocket);
            await using var listener = LocalIpcTransport.CreateListener(endpoint);

            var acceptTask = listener.AcceptAsync();
            await using var clientConn = await LocalIpcTransport.CreateClient().ConnectAsync(endpoint);
            await using var serverConn = await acceptTask;

            var payload = "hello-unix"u8.ToArray();
            await clientConn.SendAsync(new LocalIpcFrame(1, "request", "ping", payload));

            LocalIpcFrame? received = null;
            await foreach (var frame in serverConn.ReadAllAsync())
            {
                received = frame;
                break;
            }

            await Assert.That(received).IsNotNull();
            await Assert.That(received!.Payload).IsEquivalentTo(payload);

            // Double-dispose covers already-disposed branch.
            await clientConn.DisposeAsync();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Test]
    public async Task UnixDomainSocket_listener_deletes_stale_socket_file()
    {
        var path = Path.Combine(Path.GetTempPath(), $"novolis-ipc-stale-{Guid.NewGuid():N}.sock");
        await File.WriteAllTextAsync(path, "stale");
        try
        {
            var endpoint = new LocalIpcEndpoint(path, LocalIpcTransportKind.UnixDomainSocket);
            await using var listener = LocalIpcTransport.CreateListener(endpoint);
            await Assert.That(listener).IsNotNull();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Test]
    public async Task Auto_kind_resolves_platform_default_listener()
    {
        LocalIpcEndpoint endpoint;
        if (OperatingSystem.IsWindows())
        {
            endpoint = new LocalIpcEndpoint($"novolis-auto-{Guid.NewGuid():N}", LocalIpcTransportKind.Auto);
        }
        else
        {
            var path = Path.Combine(Path.GetTempPath(), $"novolis-auto-{Guid.NewGuid():N}.sock");
            endpoint = new LocalIpcEndpoint(path, LocalIpcTransportKind.Auto);
        }

        await using var listener = LocalIpcTransport.CreateListener(endpoint);
        await Assert.That(listener).IsNotNull();
    }
}

public sealed class HttpCoverageGapTests
{
    [Test]
    [Obsolete("Covers remaining Frank-prefixed HTTP DI overloads.")]
    public async Task Frank_single_argument_overloads_register()
    {
#pragma warning disable CS0618
        var servicesA = new ServiceCollection();
        servicesA.ConfigureTestHttpHandler(new StubHttpMessageHandler());
        servicesA.AddFrankHttp();
        await using var spA = servicesA.BuildServiceProvider();
        await Assert.That(spA.GetRequiredService<IRestClient>()).IsNotNull();

        var servicesB = new ServiceCollection();
        servicesB.ConfigureTestHttpHandler(new StubHttpMessageHandler());
        servicesB.AddFrankHttp(e => e.AddEnricher<HeaderEnricher>());
        await using var spB = servicesB.BuildServiceProvider();
        await Assert.That(spB.GetRequiredService<IRestClient>()).IsNotNull();

        var servicesC = new ServiceCollection();
        servicesC.ConfigureTestHttpHandler(new StubHttpMessageHandler());
        servicesC.AddFrankHttp(a => a.AddAuthentication<MarkerAuth>());
        await using var spC = servicesC.BuildServiceProvider();
        await Assert.That(spC.GetRequiredService<IRestClient>()).IsNotNull();
#pragma warning restore CS0618
    }

    [Test]
    public async Task PostAsync_untyped_returns_response_message()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);
        var client = new RestClient(http, [], []);

        using var response = await client.PostAsync("https://api.test/echo", new { n = 1 }, CancellationToken.None);
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(handler.SentRequests[0].Method).IsEqualTo(HttpMethod.Post);
    }

    private sealed class HeaderEnricher : IRequestEnricher
    {
        public void Enrich(HttpRequestMessage request) =>
            request.Headers.TryAddWithoutValidation("X-Test-Enrich", "yes");
    }

    private sealed class MarkerAuth : IHttpAuthentication
    {
        public Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation("X-Marker-Auth", "1");
            return Task.CompletedTask;
        }
    }
}
