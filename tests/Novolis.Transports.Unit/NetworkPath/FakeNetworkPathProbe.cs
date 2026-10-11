using Novolis.Transports.NetworkPath;

namespace Novolis.Transports.Unit.NetworkPath;

internal sealed class FakeNetworkPathProbe(
    IReadOnlyDictionary<string, TimeSpan?> durations) : INetworkPathProbe
{
    public Task<NetworkProbeSample> ProbeAsync(
        NetworkAdapterInfo adapter,
        NetworkProbeTarget target,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var duration = durations[adapter.Id];
        return Task.FromResult(
            duration is null
                ? new NetworkProbeSample(null, false, adapter.Addresses.FirstOrDefault(), "fixture failure")
                : new NetworkProbeSample(duration, true, adapter.Addresses.FirstOrDefault(), null));
    }
}
