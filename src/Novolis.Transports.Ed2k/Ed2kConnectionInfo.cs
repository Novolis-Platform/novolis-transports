namespace Novolis.Transports.Ed2k;

/// <summary>
/// The result of a successful server login.
/// </summary>
public sealed record Ed2kConnectionInfo(
    Ed2kServerEndpoint Server,
    uint ClientId,
    bool IsHighId,
    int AssignedPort);
