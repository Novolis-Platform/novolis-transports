namespace Novolis.Transports.Torrent.Trackers;

/// <summary>
/// Reads the daily lists from XIU2/TrackersListCollection.
/// </summary>
public sealed class Xiu2TrackerSource : TorrentTrackerSourceBase
{
    private static readonly Uri DefaultBaseAddress =
        new(
            "https://raw.githubusercontent.com/XIU2/TrackersListCollection/master/",
            UriKind.Absolute);

    /// <summary>
    /// Creates an XIU2 TrackersListCollection source.
    /// </summary>
    /// <param name="httpClient">The caller-configured HTTP client.</param>
    /// <param name="baseAddress">
    /// The raw repository base address.
    /// </param>
    public Xiu2TrackerSource(
        HttpClient httpClient,
        Uri? baseAddress = null)
        : base(
            httpClient,
            baseAddress ?? DefaultBaseAddress,
            TorrentTrackerSourceKind.Xiu2TrackersListCollection)
    {
    }

    /// <inheritdoc />
    public override Task<TorrentTrackerList> GetAsync(
        TorrentTrackerListKind listKind = TorrentTrackerListKind.Best,
        CancellationToken cancellationToken = default)
    {
        var path = listKind switch
        {
            TorrentTrackerListKind.Best => "best.txt",
            TorrentTrackerListKind.All => "all.txt",
            TorrentTrackerListKind.Http => "http.txt",
            TorrentTrackerListKind.NoHttp => "nohttp.txt",
            _ => throw new NotSupportedException(
                $"XIU2/TrackersListCollection does not expose a {listKind} list through this adapter."),
        };

        return this.GetTextListAsync(listKind, path, cancellationToken);
    }
}
