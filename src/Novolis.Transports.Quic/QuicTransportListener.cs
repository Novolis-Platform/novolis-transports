using System.Net;
using System.Net.Quic;
using System.Runtime.Versioning;

namespace Novolis.Transports.Quic;

/// <summary>Accepts secure QUIC connections.</summary>
[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class QuicTransportListener : ITransportListener
{
    private readonly QuicListener _listener;
    private int _disposed;

    private QuicTransportListener(
        QuicListener listener,
        QuicTransportOptions options)
    {
        _listener = listener;
        LocalEndPoint = listener.LocalEndPoint
            ?? throw new InvalidOperationException(
                "QUIC listener did not expose a local endpoint.");
    }

    /// <inheritdoc />
    public EndPoint LocalEndPoint { get; }

    /// <summary>Gets whether the runtime can accept QUIC connections.</summary>
    public static bool IsSupported => QuicListener.IsSupported;

    /// <summary>Starts a secure QUIC listener.</summary>
    public static async ValueTask<QuicTransportListener> ListenAsync(
        IPEndPoint endpoint,
        QuicTransportOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(options);
        if (!QuicListener.IsSupported)
        {
            throw new PlatformNotSupportedException(
                "The current runtime does not provide a QUIC listener.");
        }

        options.Validate();
        var listener = await QuicListener.ListenAsync(
                new QuicListenerOptions
                {
                    ListenEndPoint = endpoint,
                    ApplicationProtocols =
                    [
                        new System.Net.Security.SslApplicationProtocol(
                            options.ApplicationProtocol),
                    ],
                    ConnectionOptionsCallback =
                        (_, _, _) =>
                        {
                            var connectionOptions =
                                new QuicServerConnectionOptions
                                {
                                    ServerAuthenticationOptions =
                                        options.CreateServerAuthentication(),
                                    DefaultCloseErrorCode = 0,
                                    DefaultStreamErrorCode = 0,
                                    HandshakeTimeout =
                                        options.HandshakeTimeout,
                                    IdleTimeout = options.IdleTimeout,
                                    KeepAliveInterval =
                                        options.KeepAliveInterval,
                                    MaxInboundBidirectionalStreams =
                                        options.MaxInboundBidirectionalStreams,
                                    MaxInboundUnidirectionalStreams =
                                        options.MaxInboundUnidirectionalStreams,
                                };
                            return ValueTask.FromResult(connectionOptions);
                        },
                },
                cancellationToken)
            .ConfigureAwait(false);
        return new QuicTransportListener(listener, options);
    }

    /// <inheritdoc />
    public async ValueTask<ITransportConnection> AcceptConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);
        var connection = await _listener.AcceptConnectionAsync(
                cancellationToken)
            .ConfigureAwait(false);
        return QuicTransportConnection.FromAcceptedConnection(connection);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        await _listener.DisposeAsync().ConfigureAwait(false);
    }
}
