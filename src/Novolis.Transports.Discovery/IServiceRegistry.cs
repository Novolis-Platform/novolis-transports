namespace Novolis.Transports.Discovery;

/// <summary>Registers service leases and reads the current cohort map.</summary>
public interface IServiceRegistry : IServiceDiscoverySource
{
    /// <summary>Creates or replaces a service lease.</summary>
    Task RegisterAsync(
        ServiceRegistration registration,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    /// <summary>Renews an existing service lease.</summary>
    Task RenewAsync(
        ServiceRegistration registration,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a service lease.</summary>
    Task UnregisterAsync(
        ServiceInstanceKey key,
        CancellationToken cancellationToken = default);
}
