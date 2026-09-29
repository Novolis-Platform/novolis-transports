using System.Net;

namespace Novolis.Transports.Datagrams;

/// <summary>
/// Compatibility facade over <see cref="Novolis.Transports.Udp.UdpDatagramChannel"/>.
/// </summary>
public sealed class UdpDatagramChannel : IAsyncDisposable
{
    private readonly Novolis.Transports.Udp.UdpDatagramChannel _inner;

    /// <summary>Binds a UDP channel to the requested local endpoint.</summary>
    public UdpDatagramChannel(IPEndPoint localEndpoint)
    {
        ArgumentNullException.ThrowIfNull(localEndpoint);
        _inner = new Novolis.Transports.Udp.UdpDatagramChannel(localEndpoint);
    }

    /// <summary>Gets the actual local endpoint.</summary>
    public IPEndPoint LocalEndpoint => _inner.LocalEndPoint;

    /// <summary>Sends a datagram to an endpoint.</summary>
    public ValueTask<int> SendAsync(
        ReadOnlyMemory<byte> payload,
        IPEndPoint endpoint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        return _inner.SendAsync(payload, endpoint, cancellationToken);
    }

    /// <summary>Receives the next datagram.</summary>
    public async ValueTask<UdpDatagram> ReceiveAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _inner.ReceiveAsync(cancellationToken)
            .ConfigureAwait(false);
        return new UdpDatagram(
            (IPEndPoint)result.RemoteEndpoint,
            result.Payload);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => _inner.DisposeAsync();
}

/// <summary>A received UDP payload and its sender.</summary>
public sealed record UdpDatagram(IPEndPoint RemoteEndpoint, byte[] Payload);
