using DnsClient;
using DnsClient.Protocol;

namespace Novolis.Transports.Discovery;

/// <summary>
/// Resolves cohort service instances from DNS SRV records and optional TXT
/// metadata.
/// </summary>
/// <remarks>
/// A query for service <c>host</c> in cohort <c>reach</c> and domain
/// <c>example.internal</c> resolves
/// <c>_host._tcp.reach.example.internal</c>.
/// </remarks>
public sealed class DnsSrvServiceDiscovery : IServiceDiscoverySource
{
    private readonly DnsSrvServiceDiscoveryOptions _options;
    private readonly LookupClient _client;

    /// <summary>Creates a DNS SRV discovery source.</summary>
    public DnsSrvServiceDiscovery(
        DnsSrvServiceDiscoveryOptions options,
        LookupClient? client = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Protocol)
            || string.IsNullOrWhiteSpace(options.EndpointScheme))
        {
            throw new ArgumentException(
                "DNS protocol and endpoint scheme are required.",
                nameof(options));
        }
        if (!Uri.CheckSchemeName(options.EndpointScheme))
            throw new ArgumentException(
                "The endpoint scheme is invalid.",
                nameof(options));

        _options = options;
        _client = client ?? new LookupClient();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ServiceRegistration>> DiscoverAsync(
        ServiceDiscoveryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ServiceName is null)
            throw new ArgumentException(
                "DNS SRV discovery requires a service name.",
                nameof(query));

        cancellationToken.ThrowIfCancellationRequested();
        var serviceName = BuildServiceName(query);
        var result = await _client.QueryAsync(
                serviceName,
                QueryType.SRV,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var metadata = _options.IncludeTxtMetadata
            ? await ReadMetadataAsync(serviceName, cancellationToken)
                .ConfigureAwait(false)
            : new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

        var registrations = new List<ServiceRegistration>();
        foreach (var record in result.Answers.OfType<SrvRecord>())
        {
            var target = record.Target.ToString().TrimEnd('.');
            if (target.Length == 0)
                continue;

            var endpoint = new UriBuilder(
                _options.EndpointScheme,
                target,
                record.Port).Uri;
            var instanceId =
                $"{target}:{record.Port}:{record.Priority}:{record.Weight}";
            registrations.Add(
                new ServiceRegistration(
                    query.Cohort,
                    query.ServiceName,
                    instanceId,
                    new[]
                    {
                        new ServiceEndpoint(
                            endpoint,
                            record.Priority,
                            record.Weight),
                    },
                    metadata));
        }

        return registrations;
    }

    private async Task<Dictionary<string, string>> ReadMetadataAsync(
        string serviceName,
        CancellationToken cancellationToken)
    {
        var result = await _client.QueryAsync(
                serviceName,
                QueryType.TXT,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var metadata = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var record in result.Answers.OfType<TxtRecord>())
        {
            foreach (var value in record.Text)
            {
                var separator = value.IndexOf('=');
                if (separator <= 0)
                    continue;

                var key = value[..separator].Trim();
                if (key.Length == 0)
                    continue;
                metadata[key] = value[(separator + 1)..];
            }
        }

        return metadata;
    }

    private string BuildServiceName(ServiceDiscoveryQuery query) =>
        "_"
        + NormalizeDnsLabel(query.ServiceName!)
        + "._"
        + NormalizeDnsLabel(_options.Protocol)
        + "."
        + NormalizeDnsLabel(query.Cohort)
        + "."
        + _options.Domain;

    private static string NormalizeDnsLabel(string value)
    {
        var normalized = value.Trim().Trim('_').Trim('.');
        if (normalized.Length == 0
            || normalized.Any(character =>
                !(char.IsLetterOrDigit(character)
                    || character is '-' or '_')))
        {
            throw new ArgumentException(
                $"'{value}' is not a valid DNS service label.",
                nameof(value));
        }

        return normalized.ToLowerInvariant();
    }
}
