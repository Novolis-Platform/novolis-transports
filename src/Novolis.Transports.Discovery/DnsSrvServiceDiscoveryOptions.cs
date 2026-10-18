namespace Novolis.Transports.Discovery;

/// <summary>Configuration for DNS SRV/TXT service discovery.</summary>
public sealed record DnsSrvServiceDiscoveryOptions
{
    /// <summary>Creates DNS discovery options.</summary>
    public DnsSrvServiceDiscoveryOptions(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException(
                "A DNS domain is required.",
                nameof(domain));

        Domain = domain.Trim().TrimEnd('.');
        if (Domain.Length == 0)
            throw new ArgumentException(
                "A DNS domain is required.",
                nameof(domain));
    }

    /// <summary>Gets the DNS suffix below which cohorts are published.</summary>
    public string Domain { get; }

    /// <summary>Gets the SRV protocol label.</summary>
    public string Protocol { get; init; } = "tcp";

    /// <summary>Gets the URI scheme used for returned endpoints.</summary>
    public string EndpointScheme { get; init; } = "tcp";

    /// <summary>Gets whether TXT metadata should be queried.</summary>
    public bool IncludeTxtMetadata { get; init; } = true;
}
