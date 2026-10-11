namespace Novolis.Transports.NetworkPath;

/// <summary>Runs one bound probe against a network adapter.</summary>
public interface INetworkPathProbe
{
    Task<NetworkProbeSample> ProbeAsync(
        NetworkAdapterInfo adapter,
        NetworkProbeTarget target,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);
}
