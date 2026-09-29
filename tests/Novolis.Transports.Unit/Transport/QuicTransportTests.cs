using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Versioning;
using System.Text;
using Novolis.Transports;
using Novolis.Transports.Quic;
using TUnit.Core;

namespace Novolis.Transports.Unit.Transport;

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class QuicTransportTests
{
    [Test]
    public async Task Quic_capability_probe_reports_streams_without_datagrams()
    {
        await Assert.That(QuicTransportCapabilities.SupportsDatagrams).IsFalse();
        await Assert.That(
                QuicTransportCapabilities.AvailableCapabilities
                    .HasFlag(TransportCapabilities.ReliableOrderedStream))
            .IsEqualTo(QuicTransportCapabilities.SupportsStreams);
    }

    [Test]
    public async Task Quic_stream_round_trips_when_runtime_supports_quic()
    {
        if (!QuicTransportConnection.IsSupported
            || !QuicTransportListener.IsSupported)
        {
            return;
        }

        if (!string.Equals(
                Environment.GetEnvironmentVariable(
                    "NOVOLIS_RUN_NATIVE_QUIC_LOOPBACK"),
                "1",
                StringComparison.Ordinal))
        {
            Skip.Test(
                "Native QUIC loopback is opt-in because some Windows MsQuic "
                + "runtimes do not abort a failed local certificate handshake.");
            return;
        }

        using var certificate = CreateCertificate();
        var fingerprint = certificate.GetCertHashString(
            HashAlgorithmName.SHA256);
        var options = new QuicTransportOptions
        {
            ServerCertificate = certificate,
            ClientCertificateValidationCallback =
                (_, peerCertificate, _, _) =>
                    peerCertificate is not null
                    && string.Equals(
                        peerCertificate.GetCertHashString(
                            HashAlgorithmName.SHA256),
                        fingerprint,
                        StringComparison.OrdinalIgnoreCase),
        };
        await using var listener = await QuicTransportListener.ListenAsync(
            new IPEndPoint(IPAddress.Loopback, 0),
            options);
        using var timeout = new CancellationTokenSource(
            TimeSpan.FromSeconds(10));
        var acceptTask = listener.AcceptConnectionAsync(timeout.Token).AsTask();
        QuicTransportConnection client;
        try
        {
            client = await QuicTransportConnection.ConnectAsync(
                (IPEndPoint)listener.LocalEndPoint,
                options with { TargetHost = "localhost" },
                timeout.Token);
        }
        catch (Exception clientException)
        {
            try
            {
                await acceptTask.WaitAsync(TimeSpan.FromSeconds(1));
            }
            catch (Exception serverException)
            {
                throw new AggregateException(
                    "QUIC client and listener failed during handshake.",
                    clientException,
                    serverException);
            }

            throw;
        }

        await using var clientLease = client;
        await using var clientStream =
            await client.OpenBidirectionalStreamAsync();
        await using var server = await acceptTask;
        await using var serverStream =
            await server.AcceptInboundStreamAsync();

        var payload = Encoding.UTF8.GetBytes("reach-quic");
        await clientStream.Stream.WriteAsync(payload);
        await clientStream.Stream.FlushAsync();
        var received = new byte[payload.Length];
        var offset = 0;
        while (offset < received.Length)
        {
            var read = await serverStream.Stream.ReadAsync(
                received.AsMemory(offset));
            if (read == 0)
                break;
            offset += read;
        }

        await Assert.That(received).IsEquivalentTo(payload);
    }

    private static X509Certificate2 CreateCertificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            new X500DistinguishedName("CN=localhost"),
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        var names = new SubjectAlternativeNameBuilder();
        names.AddDnsName("localhost");
        names.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(names.Build());
        using var generated = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddMinutes(10));
        var pfx = generated.Export(
            X509ContentType.Pfx,
            "reach-test");
        return X509CertificateLoader.LoadPkcs12(
            pfx,
            "reach-test",
            X509KeyStorageFlags.UserKeySet
                | X509KeyStorageFlags.PersistKeySet,
            null);
    }
}
