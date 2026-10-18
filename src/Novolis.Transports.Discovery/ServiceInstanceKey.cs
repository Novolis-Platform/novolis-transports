namespace Novolis.Transports.Discovery;

/// <summary>Identifies one service instance within a cohort.</summary>
public readonly record struct ServiceInstanceKey
{
    /// <summary>Creates a service instance key.</summary>
    public ServiceInstanceKey(
        string cohort,
        string serviceName,
        string instanceId)
    {
        if (string.IsNullOrWhiteSpace(cohort))
            throw new ArgumentException(
                "A cohort is required.",
                nameof(cohort));
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException(
                "A service name is required.",
                nameof(serviceName));
        if (string.IsNullOrWhiteSpace(instanceId))
            throw new ArgumentException(
                "An instance id is required.",
                nameof(instanceId));

        Cohort = cohort.Trim().ToLowerInvariant();
        ServiceName = serviceName.Trim().ToLowerInvariant();
        InstanceId = instanceId.Trim();
    }

    /// <summary>Gets the logical cohort.</summary>
    public string Cohort { get; }

    /// <summary>Gets the service name.</summary>
    public string ServiceName { get; }

    /// <summary>Gets the stable instance identifier.</summary>
    public string InstanceId { get; }
}
