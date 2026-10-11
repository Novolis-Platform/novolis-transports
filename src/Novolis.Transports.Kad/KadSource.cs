using System.Net;

namespace Novolis.Transports.Kad;

/// <summary>
/// A source record returned by a Kad keyword or file lookup.
/// </summary>
public sealed record KadSource(
    KadId NodeId,
    IPEndPoint Endpoint,
    int TcpPort);
