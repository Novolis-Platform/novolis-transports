namespace Novolis.Transports.NetworkPath;

/// <summary>Composes adapter inventory, route resolution, and bound path probes.</summary>
public sealed class NetworkPathInspector
{
    private readonly INetworkAdapterProvider adapterProvider;
    private readonly INetworkRouteResolver routeResolver;
    private readonly INetworkPathProbe pathProbe;

    public NetworkPathInspector(
        INetworkAdapterProvider? adapterProvider = null,
        INetworkRouteResolver? routeResolver = null,
        INetworkPathProbe? pathProbe = null)
    {
        this.adapterProvider = adapterProvider ?? new SystemNetworkAdapterProvider();
        this.routeResolver = routeResolver ?? new SocketNetworkRouteResolver();
        this.pathProbe = pathProbe ?? new TcpNetworkPathProbe();
    }

    public async Task<NetworkPathReport> InspectAsync(
        NetworkProbeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Validate();

        var adapters = this.adapterProvider.GetAdapters();
        var route = await this.routeResolver
            .ResolveAsync(request.Target, cancellationToken)
            .ConfigureAwait(false);
        var selectedAdapter = FindAdapter(adapters, route.SourceAddress);
        if (selectedAdapter is not null)
        {
            route = route with { AdapterId = selectedAdapter.Id };
        }

        var paths = adapters
            .Select(adapter => Classify(adapter, selectedAdapter))
            .ToArray();
        var candidates = request.ProbeAllAdapters
            ? paths.Where(path => path.IsProbeCandidate).ToArray()
            : paths
                .Where(path => selectedAdapter is not null &&
                               string.Equals(
                                   path.Adapter.Id,
                                   selectedAdapter.Id,
                                   StringComparison.Ordinal))
                .ToArray();
        var results = new List<NetworkProbeResult>(candidates.Length);

        foreach (var path in candidates)
        {
            var samples = new List<NetworkProbeSample>(request.Attempts);
            for (var attempt = 0; attempt < request.Attempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                samples.Add(
                    await this.pathProbe
                        .ProbeAsync(
                            path.Adapter,
                            request.Target,
                            request.EffectiveTimeout,
                            cancellationToken)
                        .ConfigureAwait(false));
            }

            var sampleArray = samples.ToArray();
            results.Add(
                new NetworkProbeResult(
                    path,
                    request.Target,
                    sampleArray,
                    NetworkProbeStatistics.From(sampleArray)));
        }

        var selectedProbe = results
            .Where(result => result.Statistics.IsHealthy)
            .OrderBy(result => result.Statistics.Median ?? request.EffectiveTimeout)
            .ThenByDescending(result => result.Statistics.Successes)
            .FirstOrDefault();

        return new NetworkPathReport(
            DateTimeOffset.UtcNow,
            request.Target,
            route,
            paths,
            results,
            selectedProbe);
    }

    private static NetworkAdapterPath Classify(
        NetworkAdapterInfo adapter,
        NetworkAdapterInfo? selectedAdapter)
    {
        var evidence = adapter.TunnelEvidence.ToList();
        if (!adapter.IsOperational)
        {
            evidence.Add($"operational status is {adapter.Status}");
            return new NetworkAdapterPath(adapter, NetworkPathKind.Disconnected, evidence);
        }

        if (selectedAdapter is not null &&
            string.Equals(adapter.Id, selectedAdapter.Id, StringComparison.Ordinal))
        {
            evidence.Add("the operating system selected this adapter's source address");
            if (adapter.IsTunnelCandidate)
                evidence.Add("the selected path has tunnel-like evidence");
            return new NetworkAdapterPath(adapter, NetworkPathKind.DefaultEgress, evidence);
        }

        if (adapter.IsTunnelCandidate)
        {
            evidence.Add("the adapter has tunnel-like evidence but is not the selected path");
            return new NetworkAdapterPath(adapter, NetworkPathKind.TunnelCandidate, evidence);
        }

        if (!adapter.HasDefaultGateway)
            evidence.Add("no gateway was reported for this adapter");
        else
            evidence.Add("the adapter was not selected for the requested destination");
        return new NetworkAdapterPath(adapter, NetworkPathKind.Standby, evidence);
    }

    private static NetworkAdapterInfo? FindAdapter(
        IReadOnlyList<NetworkAdapterInfo> adapters,
        System.Net.IPAddress? sourceAddress)
    {
        if (sourceAddress is null)
            return null;

        return adapters.FirstOrDefault(
            adapter => adapter.Addresses.Any(address => address.Equals(sourceAddress)));
    }
}
