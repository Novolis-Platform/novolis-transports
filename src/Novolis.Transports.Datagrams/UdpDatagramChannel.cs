using System.Net;
using System.Net.Sockets;

namespace Novolis.Transports.Datagrams;

/// <summary>Small asynchronous UDP channel with explicit bind and send endpoints.</summary>
public sealed class UdpDatagramChannel : IAsyncDisposable
{
    private readonly UdpClient _client;

    /// <summary>Binds a UDP channel to the requested local endpoint.</summary>
    public UdpDatagramChannel(IPEndPoint localEndpoint)
    {
        ArgumentNullException.ThrowIfNull(localEndpoint);
        _client = new UdpClient(localEndpoint);
    }

    /// <summary>Gets the actual local endpoint.</summary>
    public IPEndPoint LocalEndpoint => (IPEndPoint)_client.Client.LocalEndPoint!;

    /// <summary>Sends a datagram to an endpoint.</summary>
    public ValueTask<int> SendAsync(
        ReadOnlyMemory<byte> payload,
        IPEndPoint endpoint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        return _client.SendAsync(payload, endpoint, cancellationToken);
    }

    /// <summary>Receives the next datagram.</summary>
    public async ValueTask<UdpDatagram> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        var result = await _client.ReceiveAsync(cancellationToken).ConfigureAwait(false);
        return new UdpDatagram(result.RemoteEndPoint, result.Buffer);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }
}

/// <summary>A received UDP payload and its sender.</summary>
public sealed record UdpDatagram(IPEndPoint RemoteEndpoint, byte[] Payload);
