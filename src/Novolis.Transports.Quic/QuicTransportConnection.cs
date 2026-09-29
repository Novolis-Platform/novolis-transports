using System.Net;
using System.Net.Quic;
using System.Runtime.Versioning;

namespace Novolis.Transports.Quic;

/// <summary>Adapts a secure QUIC connection to the common transport contract.</summary>
[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class QuicTransportConnection : ITransportConnection
{
    private readonly QuicConnection _connection;
    private int _disposed;

    private QuicTransportConnection(QuicConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Info = new TransportConnectionInfo(
            TransportKind.Quic,
            connection.LocalEndPoint
                ?? throw new InvalidOperationException("QUIC local endpoint is unavailable."),
            connection.RemoteEndPoint
                ?? throw new InvalidOperationException("QUIC remote endpoint is unavailable."),
            TransportCapabilities.ReliableOrderedStream
                | TransportCapabilities.Secure
                | TransportCapabilities.Multiplexed
                | TransportCapabilities.ConnectionMigration);
    }

    /// <inheritdoc />
    public TransportConnectionInfo Info { get; }

    /// <summary>Gets whether the runtime can create QUIC client connections.</summary>
    public static bool IsSupported => QuicConnection.IsSupported;

    /// <summary>Connects to a secure QUIC endpoint.</summary>
    public static async ValueTask<QuicTransportConnection> ConnectAsync(
        IPEndPoint endpoint,
        QuicTransportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (!QuicConnection.IsSupported)
        {
            throw new PlatformNotSupportedException(
                "The current runtime does not provide a QUIC client.");
        }

        options ??= new QuicTransportOptions();
        var clientOptions = new QuicClientConnectionOptions
        {
            RemoteEndPoint = endpoint,
            ClientAuthenticationOptions =
                options.CreateClientAuthentication(endpoint.Address.ToString()),
            DefaultCloseErrorCode = 0,
            DefaultStreamErrorCode = 0,
            HandshakeTimeout = options.HandshakeTimeout,
            IdleTimeout = options.IdleTimeout,
            KeepAliveInterval = options.KeepAliveInterval,
            MaxInboundBidirectionalStreams =
                options.MaxInboundBidirectionalStreams,
            MaxInboundUnidirectionalStreams =
                options.MaxInboundUnidirectionalStreams,
        };
        var connection = await QuicConnection.ConnectAsync(
                clientOptions,
                cancellationToken)
            .ConfigureAwait(false);
        return new QuicTransportConnection(connection);
    }

    /// <summary>Wraps an accepted QUIC connection.</summary>
    internal static QuicTransportConnection FromAcceptedConnection(
        QuicConnection connection) =>
        new(connection);

    /// <inheritdoc />
    public async ValueTask<ITransportStream> OpenBidirectionalStreamAsync(
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        var stream = await _connection.OpenOutboundStreamAsync(
                QuicStreamType.Bidirectional,
                cancellationToken)
            .ConfigureAwait(false);
        return new QuicTransportStream(stream);
    }

    /// <inheritdoc />
    public async ValueTask<ITransportStream> AcceptInboundStreamAsync(
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        var stream = await _connection.AcceptInboundStreamAsync(
                cancellationToken)
            .ConfigureAwait(false);
        return new QuicTransportStream(stream);
    }

    /// <inheritdoc />
    public async ValueTask CloseAsync(
        long errorCode = 0,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        await _connection.CloseAsync(errorCode, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return ValueTask.CompletedTask;

        return _connection.DisposeAsync();
    }

    private void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);
}
