using System.Net;

namespace Novolis.Transports;

/// <summary>Immutable information about an established transport connection.</summary>
public sealed record TransportConnectionInfo(
    TransportKind Kind,
    EndPoint LocalEndPoint,
    EndPoint RemoteEndPoint,
    TransportCapabilities Capabilities);
