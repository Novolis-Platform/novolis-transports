namespace Novolis.Transports.Discovery;

/// <summary>One reachable address advertised by a service instance.</summary>
public sealed record ServiceEndpoint
{
    /// <summary>Creates an endpoint.</summary>
    public ServiceEndpoint(
        Uri address,
        ushort priority = 0,
        ushort weight = 0)
    {
        ArgumentNullException.ThrowIfNull(address);
        if (!address.IsAbsoluteUri)
            throw new ArgumentException(
                "The endpoint address must be absolute.",
                nameof(address));

        Address = address;
        Priority = priority;
        Weight = weight;
    }

    /// <summary>Gets the absolute endpoint URI.</summary>
    public Uri Address { get; }

    /// <summary>Gets the SRV-style failover priority.</summary>
    public ushort Priority { get; }

    /// <summary>Gets the SRV-style selection weight.</summary>
    public ushort Weight { get; }
}
