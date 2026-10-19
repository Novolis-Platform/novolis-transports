namespace Novolis.Transports.Torrent.Trackers;

/// <summary>
/// Coordinates the supported public tracker-list services.
/// </summary>
public sealed class TorrentTrackerCatalog
{
    /// <summary>
    /// Creates a catalog backed by newTrackon, ngosang/trackerslist,
    /// and XIU2/TrackersListCollection.
    /// </summary>
    /// <param name="httpClient">The caller-configured HTTP client.</param>
    public TorrentTrackerCatalog(HttpClient httpClient)
        : this(
            [
                new NewTrackonTrackerSource(httpClient),
                new NgosangTrackerSource(httpClient),
                new Xiu2TrackerSource(httpClient),
            ])
    {
    }

    /// <summary>
    /// Creates a catalog from caller-supplied sources.
    /// </summary>
    /// <param name="sources">The tracker-list sources.</param>
    public TorrentTrackerCatalog(IEnumerable<ITorrentTrackerSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        this.Sources = sources.ToArray();
        if (this.Sources.Count == 0)
            throw new ArgumentException(
                "At least one tracker-list source is required.",
                nameof(sources));
    }

    /// <summary>
    /// Gets the configured sources.
    /// </summary>
    public IReadOnlyList<ITorrentTrackerSource> Sources { get; }

    /// <summary>
    /// Gets a source by identity.
    /// </summary>
    public ITorrentTrackerSource this[TorrentTrackerSourceKind source]
    {
        get
        {
            return this.Sources.FirstOrDefault(candidate => candidate.Kind == source)
                   ?? throw new KeyNotFoundException(
                       $"No tracker-list source is configured for {source}.");
        }
    }

    /// <summary>
    /// Retrieves one list from one configured source.
    /// </summary>
    public Task<TorrentTrackerList> GetAsync(
        TorrentTrackerSourceKind source,
        TorrentTrackerListKind listKind = TorrentTrackerListKind.Best,
        CancellationToken cancellationToken = default)
    {
        return this[source].GetAsync(listKind, cancellationToken);
    }

    /// <summary>
    /// Retrieves the requested list from every configured source.
    /// </summary>
    public async Task<IReadOnlyList<TorrentTrackerList>> GetAllAsync(
        TorrentTrackerListKind listKind = TorrentTrackerListKind.Best,
        CancellationToken cancellationToken = default)
    {
        var lists = await Task.WhenAll(
                this.Sources.Select(source => source.GetAsync(
                    listKind,
                    cancellationToken)))
            .ConfigureAwait(false);
        return lists;
    }

    /// <summary>
    /// Retrieves and de-duplicates tracker URLs from every configured source.
    /// </summary>
    public async Task<IReadOnlyList<Uri>> GetDistinctTrackersAsync(
        TorrentTrackerListKind listKind = TorrentTrackerListKind.Best,
        CancellationToken cancellationToken = default)
    {
        var lists = await this.GetAllAsync(listKind, cancellationToken)
            .ConfigureAwait(false);
        var trackers = new List<Uri>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var list in lists)
        {
            foreach (var tracker in list.Trackers)
            {
                if (seen.Add(tracker.AbsoluteUri))
                    trackers.Add(tracker);
            }
        }

        return trackers;
    }
}
