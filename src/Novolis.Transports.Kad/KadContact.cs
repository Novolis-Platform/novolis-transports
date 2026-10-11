using System.Net;

namespace Novolis.Transports.Kad;

/// <summary>
/// A contact advertised by a Kad node.
/// </summary>
public sealed record KadContact(
    KadId Id,
    IPEndPoint Endpoint,
    int TcpPort,
    byte Version = 0x08)
{
    public int UdpPort => this.Endpoint.Port;
}
