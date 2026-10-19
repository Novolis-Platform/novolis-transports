namespace Novolis.Transports.Torrent.Trackers;

/// <summary>
/// A tracker list retrieved from a public tracker-list service.
/// </summary>
public sealed record TorrentTrackerList
{
    /// <summary>
    /// Creates a tracker-list snapshot.
    /// </summary>
    public TorrentTrackerList(
        TorrentTrackerSourceKind source,
        TorrentTrackerListKind kind,
        Uri endpoint,
        DateTimeOffset retrievedAt,
        IEnumerable<Uri> trackers)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(trackers);
        if (!endpoint.IsAbsoluteUri
            || endpoint.Scheme is not ("http" or "https"))
        {
            throw new ArgumentException(
                "The tracker-list endpoint must be an absolute HTTP or HTTPS URI.",
                nameof(endpoint));
        }

        this.Source = source;
        this.Kind = kind;
        this.Endpoint = endpoint;
        this.RetrievedAt = retrievedAt;
        this.Trackers = trackers.ToArray();
    }

    /// <summary>
    /// Gets the source service.
    /// </summary>
    public TorrentTrackerSourceKind Source { get; }

    /// <summary>
    /// Gets the requested list kind.
    /// </summary>
    public TorrentTrackerListKind Kind { get; }

    /// <summary>
    /// Gets the URL used to retrieve the list.
    /// </summary>
    public Uri Endpoint { get; }

    /// <summary>
    /// Gets the retrieval timestamp in UTC.
    /// </summary>
    public DateTimeOffset RetrievedAt { get; }

    /// <summary>
    /// Gets the parsed announce URLs.
    /// </summary>
    public IReadOnlyList<Uri> Trackers { get; }
}
