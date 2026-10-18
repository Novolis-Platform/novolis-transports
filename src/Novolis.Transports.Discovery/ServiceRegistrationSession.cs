namespace Novolis.Transports.Discovery;

/// <summary>
/// Keeps one registration alive by renewing its lease until cancellation.
/// </summary>
public sealed class ServiceRegistrationSession
{
    private readonly IServiceRegistry _registry;
    private readonly ServiceRegistration _registration;
    private readonly TimeSpan _leaseDuration;
    private readonly TimeSpan _renewalPeriod;

    /// <summary>Creates a registration session.</summary>
    public ServiceRegistrationSession(
        IServiceRegistry registry,
        ServiceRegistration registration,
        TimeSpan leaseDuration,
        TimeSpan renewalPeriod)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            leaseDuration,
            TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            renewalPeriod,
            TimeSpan.Zero);
        if (renewalPeriod >= leaseDuration)
            throw new ArgumentException(
                "The renewal period must be shorter than the lease duration.",
                nameof(renewalPeriod));

        _registry = registry;
        _registration = registration;
        _leaseDuration = leaseDuration;
        _renewalPeriod = renewalPeriod;
    }

    /// <summary>Registers, renews, and finally unregisters the service.</summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await _registry.RegisterAsync(
                _registration,
                _leaseDuration,
                cancellationToken)
            .ConfigureAwait(false);

        try
        {
            using var timer = new PeriodicTimer(_renewalPeriod);
            while (await timer.WaitForNextTickAsync(cancellationToken)
                       .ConfigureAwait(false))
            {
                await _registry.RenewAsync(
                        _registration,
                        _leaseDuration,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            try
            {
                await _registry.UnregisterAsync(
                        _registration.Key,
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch when (cancellationToken.IsCancellationRequested)
            {
                // Lease expiry is the fallback when shutdown interrupts cleanup.
            }
        }
    }
}
