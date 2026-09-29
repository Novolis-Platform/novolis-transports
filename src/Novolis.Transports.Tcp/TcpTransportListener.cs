using System.Net;
using System.Net.Sockets;

namespace Novolis.Transports.Tcp;

/// <summary>Adapts a TCP listener to the common transport contract.</summary>
public sealed class TcpTransportListener : ITransportListener
{
    private readonly TcpListener _listener;
    private int _disposed;

    /// <summary>Starts a TCP listener on the requested endpoint.</summary>
    public TcpTransportListener(IPEndPoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        _listener = new TcpListener(endpoint);
        _listener.Start();
        LocalEndPoint = (IPEndPoint)_listener.LocalEndpoint;
    }

    /// <inheritdoc />
    public EndPoint LocalEndPoint { get; }

    /// <inheritdoc />
    public async ValueTask<ITransportConnection> AcceptConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);
        var client = await _listener.AcceptTcpClientAsync(cancellationToken)
            .ConfigureAwait(false);
        client.NoDelay = true;
        return TcpTransportConnection.FromAcceptedClient(client);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            _listener.Stop();

        return ValueTask.CompletedTask;
    }
}
