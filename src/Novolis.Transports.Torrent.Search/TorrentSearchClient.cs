namespace Novolis.Transports.Torrent.Search;

/// <summary>
/// Searches one or more providers and applies common result filtering.
/// </summary>
public sealed class TorrentSearchClient
{
    /// <summary>
    /// Creates a search client for one provider.
    /// </summary>
    public TorrentSearchClient(ITorrentSearchProvider provider)
        : this([provider])
    {
    }

    /// <summary>
    /// Creates a search client for multiple providers.
    /// </summary>
    public TorrentSearchClient(IEnumerable<ITorrentSearchProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        this.Providers = providers.ToArray();
        if (this.Providers.Count == 0)
            throw new ArgumentException(
                "At least one search provider is required.",
                nameof(providers));
    }

    /// <summary>
    /// Gets the configured providers.
    /// </summary>
    public IReadOnlyList<ITorrentSearchProvider> Providers { get; }

    /// <summary>
    /// Searches all configured providers, de-duplicates results, and filters them.
    /// </summary>
    public async Task<IReadOnlyList<TorrentSearchResult>> SearchAsync(
        TorrentSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        query.Validate();

        var batches = await Task.WhenAll(
                this.Providers.Select(provider => provider.SearchAsync(
                    query,
                    cancellationToken)))
            .ConfigureAwait(false);
        var results = batches.SelectMany(batch => batch);
        return Apply(query, results);
    }

    /// <summary>
    /// Applies the common filters and sort order to an existing result sequence.
    /// </summary>
    public static IReadOnlyList<TorrentSearchResult> Apply(
        TorrentSearchQuery query,
        IEnumerable<TorrentSearchResult> results)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(results);
        query.Validate();

        var unique = new List<TorrentSearchResult>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var result in results)
        {
            ArgumentNullException.ThrowIfNull(result);
            if (!query.Matches(result))
                continue;

            var identity = GetIdentity(result);
            if (seen.Add(identity))
                unique.Add(result);
        }

        IEnumerable<TorrentSearchResult> ordered = query.SortBy switch
        {
            TorrentSearchSort.Seeders => query.SortDescending
                ? unique.OrderByDescending(result => result.Seeders ?? -1)
                : unique.OrderBy(result => result.Seeders ?? -1),
            TorrentSearchSort.Leechers => query.SortDescending
                ? unique.OrderByDescending(result => result.Leechers ?? -1)
                : unique.OrderBy(result => result.Leechers ?? -1),
            TorrentSearchSort.Ratio => query.SortDescending
                ? unique.OrderByDescending(result => result.EffectiveRatio ?? -1m)
                : unique.OrderBy(result => result.EffectiveRatio ?? -1m),
            TorrentSearchSort.Availability => query.SortDescending
                ? unique.OrderByDescending(result => result.Availability ?? -1m)
                : unique.OrderBy(result => result.Availability ?? -1m),
            TorrentSearchSort.Size => query.SortDescending
                ? unique.OrderByDescending(result => result.SizeBytes ?? -1)
                : unique.OrderBy(result => result.SizeBytes ?? -1),
            TorrentSearchSort.PublishedAt => query.SortDescending
                ? unique.OrderByDescending(
                    result => result.PublishedAt ?? DateTimeOffset.MinValue)
                : unique.OrderBy(
                    result => result.PublishedAt ?? DateTimeOffset.MinValue),
            _ => unique,
        };

        return ordered.Take(query.Limit).ToArray();
    }

    private static string GetIdentity(TorrentSearchResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.InfoHash))
            return "hash:" + result.InfoHash;
        if (result.MagnetUri is not null)
            return "magnet:" + result.MagnetUri.AbsoluteUri;

        return string.Concat(
            "name:",
            result.Name,
            "|size:",
            result.SizeBytes?.ToString() ?? string.Empty);
    }
}
