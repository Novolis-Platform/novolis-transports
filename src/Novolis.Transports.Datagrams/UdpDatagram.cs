using System.Net;

namespace Novolis.Transports.Datagrams;

/// <summary>A received UDP payload and its sender.</summary>
public sealed record UdpDatagram(IPEndPoint RemoteEndpoint, byte[] Payload);
