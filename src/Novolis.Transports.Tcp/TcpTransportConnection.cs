using System.Net;
using System.Net.Sockets;

namespace Novolis.Transports.Tcp;

/// <summary>Adapts one TCP connection to the common transport contract.</summary>
public sealed class TcpTransportConnection : ITransportConnection
{
    private readonly TcpClient _client;
    private readonly TcpTransportStream _stream;
    private int _streamClaimed;
    private int _disposed;

    private TcpTransportConnection(TcpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _stream = new TcpTransportStream(client.GetStream());
        Info = new TransportConnectionInfo(
            TransportKind.Tcp,
            client.Client.LocalEndPoint
                ?? throw new InvalidOperationException("TCP local endpoint is unavailable."),
            client.Client.RemoteEndPoint
                ?? throw new InvalidOperationException("TCP remote endpoint is unavailable."),
            TransportCapabilities.ReliableOrderedStream);
    }

    /// <inheritdoc />
    public TransportConnectionInfo Info { get; }

    /// <summary>Connects to a TCP endpoint.</summary>
    public static async ValueTask<TcpTransportConnection> ConnectAsync(
        IPEndPoint endpoint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        var client = new TcpClient(endpoint.AddressFamily)
        {
            NoDelay = true,
        };
        try
        {
            await client.ConnectAsync(
                    endpoint.Address,
                    endpoint.Port,
                    cancellationToken)
                .ConfigureAwait(false);
            return new TcpTransportConnection(client);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    /// <summary>Wraps an accepted TCP client.</summary>
    internal static TcpTransportConnection FromAcceptedClient(TcpClient client) =>
        new(client);

    /// <inheritdoc />
    public ValueTask<ITransportStream> OpenBidirectionalStreamAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ClaimStream();
        return ValueTask.FromResult<ITransportStream>(_stream);
    }

    /// <inheritdoc />
    public ValueTask<ITransportStream> AcceptInboundStreamAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ClaimStream();
        return ValueTask.FromResult<ITransportStream>(_stream);
    }

    /// <inheritdoc />
    public ValueTask CloseAsync(
        long errorCode = 0,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return DisposeAsync();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        await _stream.DisposeAsync().ConfigureAwait(false);
        _client.Dispose();
    }

    private void ClaimStream()
    {
        if (Interlocked.Exchange(ref _streamClaimed, 1) != 0)
        {
            throw new InvalidOperationException(
                "A TCP connection exposes one reliable stream.");
        }
    }

    private void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);
}
