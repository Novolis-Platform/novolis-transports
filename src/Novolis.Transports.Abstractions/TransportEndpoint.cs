using System.Net;

namespace Novolis.Transports;

/// <summary>Transport-independent endpoint description.</summary>
public sealed record TransportEndpoint(
    TransportKind Kind,
    EndPoint Address,
    string? Authority = null)
{
    /// <summary>Creates an endpoint with a required address.</summary>
    public TransportEndpoint(TransportKind kind, EndPoint address)
        : this(kind, address, null)
    {
    }
}
