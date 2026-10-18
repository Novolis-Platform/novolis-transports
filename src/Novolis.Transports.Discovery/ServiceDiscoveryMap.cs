namespace Novolis.Transports.Discovery;

/// <summary>
/// Thread-safe view of service instances observed from one or more sources.
/// </summary>
public sealed class ServiceDiscoveryMap
{
    private readonly object _gate = new();
    private Dictionary<ServiceInstanceKey, ServiceRegistration> _services = new();

    /// <summary>Raised after the map has been replaced, updated, or reduced.</summary>
    public event EventHandler? Changed;

    /// <summary>Gets a stable snapshot of all currently stored instances.</summary>
    public IReadOnlyList<ServiceRegistration> Snapshot
    {
        get
        {
            lock (_gate)
                return _services.Values.ToArray();
        }
    }

    /// <summary>Gets matching instances from one cohort.</summary>
    public IReadOnlyList<ServiceRegistration> Find(
        ServiceDiscoveryQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        lock (_gate)
        {
            return _services.Values
                .Where(registration =>
                    string.Equals(
                        registration.Cohort,
                        query.Cohort,
                        StringComparison.OrdinalIgnoreCase)
                    && (query.ServiceName is null
                        || string.Equals(
                            registration.ServiceName,
                            query.ServiceName,
                            StringComparison.OrdinalIgnoreCase)))
                .ToArray();
        }
    }

    /// <summary>
    /// Queries all supplied sources concurrently and replaces the map with the
    /// combined snapshot.
    /// </summary>
    public async Task RefreshAsync(
        ServiceDiscoveryQuery query,
        IEnumerable<IServiceDiscoverySource> sources,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(sources);

        var sourceArray = sources.ToArray();
        if (sourceArray.Any(static source => source is null))
            throw new ArgumentException(
                "Discovery sources cannot contain null values.",
                nameof(sources));

        var snapshots = await Task.WhenAll(
                sourceArray.Select(source =>
                    source.DiscoverAsync(query, cancellationToken)))
            .ConfigureAwait(false);
        Replace(snapshots.SelectMany(static snapshot => snapshot));
    }

    /// <summary>Replaces the map with a new source snapshot.</summary>
    public void Replace(IEnumerable<ServiceRegistration> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);

        var next = new Dictionary<ServiceInstanceKey, ServiceRegistration>();
        foreach (var registration in registrations)
        {
            ArgumentNullException.ThrowIfNull(registration);
            next[registration.Key] = registration;
        }

        lock (_gate)
            _services = next;

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Adds or replaces one service instance.</summary>
    public void Upsert(ServiceRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        lock (_gate)
            _services[registration.Key] = registration;

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Removes one service instance if it is present.</summary>
    public bool Remove(ServiceInstanceKey key)
    {
        bool removed;
        lock (_gate)
            removed = _services.Remove(key);

        if (removed)
            Changed?.Invoke(this, EventArgs.Empty);
        return removed;
    }
}
