namespace Novolis.Transports.Discovery;

/// <summary>Reads service instances from one discovery mechanism.</summary>
public interface IServiceDiscoverySource
{
    /// <summary>Discovers the currently visible matching service instances.</summary>
    Task<IReadOnlyList<ServiceRegistration>> DiscoverAsync(
        ServiceDiscoveryQuery query,
        CancellationToken cancellationToken = default);
}
