namespace Novolis.Transports.Discovery;

/// <summary>Filters service instances returned by a discovery source.</summary>
public sealed record ServiceDiscoveryQuery
{
    /// <summary>Creates a cohort query.</summary>
    public ServiceDiscoveryQuery(
        string cohort,
        string? serviceName = null)
    {
        if (string.IsNullOrWhiteSpace(cohort))
            throw new ArgumentException(
                "A cohort is required.",
                nameof(cohort));
        if (serviceName is not null && string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException(
                "A service name cannot be blank.",
                nameof(serviceName));

        Cohort = cohort.Trim().ToLowerInvariant();
        ServiceName = serviceName?.Trim().ToLowerInvariant();
    }

    /// <summary>Gets the logical cohort to query.</summary>
    public string Cohort { get; }

    /// <summary>Gets the optional service name filter.</summary>
    public string? ServiceName { get; }
}
