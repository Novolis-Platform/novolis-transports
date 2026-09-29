using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Novolis.Transports.Quic;

/// <summary>Shared configuration for QUIC clients and listeners.</summary>
public sealed record QuicTransportOptions
{
    /// <summary>Default Reach-compatible ALPN identifier.</summary>
    public const string DefaultApplicationProtocol = "novolis-reach/1";

    /// <summary>ALPN identifier negotiated during the TLS 1.3 handshake.</summary>
    public string ApplicationProtocol { get; init; } =
        DefaultApplicationProtocol;

    /// <summary>Optional TLS server name used by a client.</summary>
    public string? TargetHost { get; init; }

    /// <summary>Server certificate used by a listener.</summary>
    public X509Certificate2? ServerCertificate { get; init; }

    /// <summary>Whether a listener requires a client certificate.</summary>
    public bool RequireClientCertificate { get; init; }

    /// <summary>Optional client-side certificate validation callback.</summary>
    public RemoteCertificateValidationCallback?
        ClientCertificateValidationCallback { get; init; }

    /// <summary>Optional server-side client-certificate validation callback.</summary>
    public RemoteCertificateValidationCallback?
        ServerCertificateValidationCallback { get; init; }

    /// <summary>Maximum time allowed for the QUIC handshake.</summary>
    public TimeSpan HandshakeTimeout { get; init; } =
        TimeSpan.FromSeconds(10);

    /// <summary>Idle timeout applied to established connections.</summary>
    public TimeSpan IdleTimeout { get; init; } =
        TimeSpan.FromMinutes(2);

    /// <summary>Optional QUIC keepalive interval.</summary>
    public TimeSpan KeepAliveInterval { get; init; } =
        TimeSpan.FromSeconds(30);

    /// <summary>Maximum inbound bidirectional streams.</summary>
    public int MaxInboundBidirectionalStreams { get; init; } = 16;

    /// <summary>Maximum inbound unidirectional streams.</summary>
    public int MaxInboundUnidirectionalStreams { get; init; } = 4;

    internal SslClientAuthenticationOptions CreateClientAuthentication(
        string targetHost)
    {
        Validate();
        return new SslClientAuthenticationOptions
        {
            TargetHost = TargetHost ?? targetHost,
            ApplicationProtocols =
            [
                new SslApplicationProtocol(ApplicationProtocol),
            ],
            RemoteCertificateValidationCallback =
                ClientCertificateValidationCallback,
        };
    }

    internal SslServerAuthenticationOptions CreateServerAuthentication()
    {
        Validate();
        return new SslServerAuthenticationOptions
        {
            ServerCertificate = ServerCertificate
                ?? throw new InvalidOperationException(
                    "A QUIC listener requires a server certificate."),
            ApplicationProtocols =
            [
                new SslApplicationProtocol(ApplicationProtocol),
            ],
            ClientCertificateRequired = RequireClientCertificate,
            RemoteCertificateValidationCallback =
                ServerCertificateValidationCallback,
        };
    }

    internal void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ApplicationProtocol);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            HandshakeTimeout,
            TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            IdleTimeout,
            TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            KeepAliveInterval,
            TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(
            MaxInboundBidirectionalStreams,
            0);
        ArgumentOutOfRangeException.ThrowIfLessThan(
            MaxInboundUnidirectionalStreams,
            0);
    }
}
