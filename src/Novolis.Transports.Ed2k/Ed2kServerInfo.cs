namespace Novolis.Transports.Ed2k;

/// <summary>
/// Metadata for a server-list entry.
/// </summary>
public sealed record Ed2kServerInfo(
    Ed2kServerEndpoint Endpoint,
    string? Name = null,
    uint? UserCount = null,
    uint? FileCount = null);
