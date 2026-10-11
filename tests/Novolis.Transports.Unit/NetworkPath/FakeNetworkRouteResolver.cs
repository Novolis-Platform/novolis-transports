using Novolis.Transports.NetworkPath;

namespace Novolis.Transports.Unit.NetworkPath;

internal sealed class FakeNetworkRouteResolver(
    System.Net.IPAddress? sourceAddress) : INetworkRouteResolver
{
    public Task<NetworkRoute> ResolveAsync(
        NetworkProbeTarget target,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new NetworkRoute(target, sourceAddress, null, null));
}
