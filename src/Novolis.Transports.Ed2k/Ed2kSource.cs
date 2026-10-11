namespace Novolis.Transports.Ed2k;

/// <summary>
/// A file source returned by a server or peer.
/// </summary>
public sealed record Ed2kSource(
    uint ClientId,
    string Address,
    int Port,
    int KadPort = 0);
