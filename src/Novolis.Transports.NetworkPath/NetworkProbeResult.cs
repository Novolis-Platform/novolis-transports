namespace Novolis.Transports.NetworkPath;

/// <summary>All samples and aggregate statistics for one adapter path.</summary>
public sealed record NetworkProbeResult(
    NetworkAdapterPath Path,
    NetworkProbeTarget Target,
    IReadOnlyList<NetworkProbeSample> Samples,
    NetworkProbeStatistics Statistics);
