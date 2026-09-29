using System.Net;

namespace Novolis.Transports;

/// <summary>Sends and receives bounded loss-tolerant datagrams.</summary>
public interface ITransportDatagramChannel : IAsyncDisposable
{
    /// <summary>Gets the largest payload accepted by this channel.</summary>
    int MaximumPayloadSize { get; }

    /// <summary>Gets the local endpoint bound by the channel.</summary>
    IPEndPoint LocalEndPoint { get; }

    /// <summary>Sends one payload to a remote endpoint.</summary>
    ValueTask<int> SendAsync(
        ReadOnlyMemory<byte> payload,
        IPEndPoint endpoint,
        CancellationToken cancellationToken = default);

    /// <summary>Receives the next accepted datagram.</summary>
    ValueTask<TransportDatagram> ReceiveAsync(
        CancellationToken cancellationToken = default);
}
