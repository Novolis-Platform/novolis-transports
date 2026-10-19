namespace Novolis.Transports.Torrent.Trackers;

/// <summary>
/// Reads live and stable tracker lists from newTrackon.
/// </summary>
public sealed class NewTrackonTrackerSource : TorrentTrackerSourceBase
{
    private static readonly Uri DefaultBaseAddress =
        new("https://newtrackon.com/", UriKind.Absolute);

    /// <summary>
    /// Creates a newTrackon source.
    /// </summary>
    /// <param name="httpClient">The caller-configured HTTP client.</param>
    /// <param name="baseAddress">
    /// The service base address. The default is <c>https://newtrackon.com/</c>.
    /// </param>
    public NewTrackonTrackerSource(
        HttpClient httpClient,
        Uri? baseAddress = null)
        : base(
            httpClient,
            baseAddress ?? DefaultBaseAddress,
            TorrentTrackerSourceKind.NewTrackon)
    {
    }

    /// <inheritdoc />
    public override Task<TorrentTrackerList> GetAsync(
        TorrentTrackerListKind listKind = TorrentTrackerListKind.Best,
        CancellationToken cancellationToken = default)
    {
        var path = listKind switch
        {
            TorrentTrackerListKind.Best or TorrentTrackerListKind.Stable => "api/stable",
            TorrentTrackerListKind.Live => "api/live",
            _ => throw new NotSupportedException(
                $"newTrackon does not expose a {listKind} list through this adapter."),
        };

        return this.GetTextListAsync(listKind, path, cancellationToken);
    }
}
