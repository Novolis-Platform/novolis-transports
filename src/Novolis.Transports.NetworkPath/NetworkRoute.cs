using System.Net;

namespace Novolis.Transports.NetworkPath;

/// <summary>The source address selected by the operating system for a destination.</summary>
public sealed record NetworkRoute(
    NetworkProbeTarget Target,
    IPAddress? SourceAddress,
    string? AdapterId,
    string? Error)
{
    public bool IsResolved => this.SourceAddress is not null;
}
