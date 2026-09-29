using System.Net;

namespace Novolis.Transports;

/// <summary>A received datagram and its remote endpoint.</summary>
public sealed record TransportDatagram(
    EndPoint RemoteEndpoint,
    byte[] Payload,
    DateTimeOffset ReceivedAt);
