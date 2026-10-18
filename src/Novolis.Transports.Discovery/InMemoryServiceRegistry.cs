using System.Collections.Concurrent;

namespace Novolis.Transports.Discovery;

/// <summary>
/// In-process lease registry useful for tests, local composition, and
/// deterministic simulations.
/// </summary>
public sealed class InMemoryServiceRegistry : IServiceRegistry
{
    private readonly ConcurrentDictionary<ServiceInstanceKey, Lease> _leases = new();
    private readonly TimeProvider _timeProvider;

    /// <summary>Creates an in-memory registry.</summary>
    public InMemoryServiceRegistry(TimeProvider? timeProvider = null) =>
        _timeProvider = timeProvider ?? TimeProvider.System;

    /// <inheritdoc />
    public Task RegisterAsync(
        ServiceRegistration registration,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ValidateLeaseDuration(leaseDuration);
        cancellationToken.ThrowIfCancellationRequested();

        _leases[registration.Key] = new Lease(
            registration,
            _timeProvider.GetUtcNow() + leaseDuration);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RenewAsync(
        ServiceRegistration registration,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ValidateLeaseDuration(leaseDuration);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_leases.ContainsKey(registration.Key))
            throw new KeyNotFoundException(
                $"Service instance '{registration.Key.InstanceId}' is not registered.");

        _leases[registration.Key] = new Lease(
            registration,
            _timeProvider.GetUtcNow() + leaseDuration);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UnregisterAsync(
        ServiceInstanceKey key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _leases.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ServiceRegistration>> DiscoverAsync(
        ServiceDiscoveryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        var now = _timeProvider.GetUtcNow();
        var visible = new List<ServiceRegistration>();
        foreach (var pair in _leases)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (pair.Value.ExpiresAt <= now)
            {
                if (_leases.TryGetValue(pair.Key, out var current)
                    && ReferenceEquals(current, pair.Value))
                {
                    _leases.TryRemove(pair.Key, out _);
                }

                continue;
            }

            if (string.Equals(
                    pair.Value.Registration.Cohort,
                    query.Cohort,
                    StringComparison.OrdinalIgnoreCase)
                && (query.ServiceName is null
                    || string.Equals(
                        pair.Value.Registration.ServiceName,
                        query.ServiceName,
                        StringComparison.OrdinalIgnoreCase)))
            {
                visible.Add(pair.Value.Registration);
            }
        }

        return Task.FromResult<IReadOnlyList<ServiceRegistration>>(
            visible
                .OrderBy(static registration => registration.ServiceName)
                .ThenBy(static registration => registration.InstanceId)
                .ToArray());
    }

    private static void ValidateLeaseDuration(TimeSpan leaseDuration) =>
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            leaseDuration,
            TimeSpan.Zero);

    private sealed class Lease
    {
        internal Lease(
            ServiceRegistration registration,
            DateTimeOffset expiresAt)
        {
            Registration = registration;
            ExpiresAt = expiresAt;
        }

        internal ServiceRegistration Registration { get; }
        internal DateTimeOffset ExpiresAt { get; }
    }
}
