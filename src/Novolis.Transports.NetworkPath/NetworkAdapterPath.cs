namespace Novolis.Transports.NetworkPath;

/// <summary>Classifies one adapter for a requested network destination.</summary>
public sealed record NetworkAdapterPath(
    NetworkAdapterInfo Adapter,
    NetworkPathKind Kind,
    IReadOnlyList<string> Evidence)
{
    public bool IsProbeCandidate =>
        Adapter.IsOperational && Adapter.Addresses.Count > 0;
}
