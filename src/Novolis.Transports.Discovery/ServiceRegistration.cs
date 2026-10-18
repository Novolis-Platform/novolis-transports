using System.Collections.ObjectModel;

namespace Novolis.Transports.Discovery;

/// <summary>Registration data published by one service instance.</summary>
public sealed class ServiceRegistration
{
    /// <summary>Creates a service registration.</summary>
    public ServiceRegistration(
        string cohort,
        string serviceName,
        string instanceId,
        IEnumerable<ServiceEndpoint> endpoints,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        Key = new ServiceInstanceKey(cohort, serviceName, instanceId);
        ArgumentNullException.ThrowIfNull(endpoints);

        var endpointArray = endpoints.ToArray();
        if (endpointArray.Length == 0)
            throw new ArgumentException(
                "At least one endpoint is required.",
                nameof(endpoints));
        if (endpointArray.Any(static endpoint => endpoint is null))
            throw new ArgumentException(
                "Endpoints cannot contain null values.",
                nameof(endpoints));

        Endpoints = Array.AsReadOnly(endpointArray);
        Metadata = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(
                metadata ?? new Dictionary<string, string>(),
                StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>Gets the service instance key.</summary>
    public ServiceInstanceKey Key { get; }

    /// <summary>Gets the logical cohort.</summary>
    public string Cohort => Key.Cohort;

    /// <summary>Gets the service name.</summary>
    public string ServiceName => Key.ServiceName;

    /// <summary>Gets the stable instance identifier.</summary>
    public string InstanceId => Key.InstanceId;

    /// <summary>Gets the reachable endpoints.</summary>
    public IReadOnlyList<ServiceEndpoint> Endpoints { get; }

    /// <summary>Gets service-specific metadata.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
