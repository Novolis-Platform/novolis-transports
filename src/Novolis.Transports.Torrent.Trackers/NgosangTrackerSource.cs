namespace Novolis.Transports.Torrent.Trackers;

/// <summary>
/// Reads the automatically refreshed lists from ngosang/trackerslist.
/// </summary>
public sealed class NgosangTrackerSource : TorrentTrackerSourceBase
{
    private static readonly Uri DefaultBaseAddress =
        new(
            "https://raw.githubusercontent.com/ngosang/trackerslist/master/",
            UriKind.Absolute);

    /// <summary>
    /// Creates an ngosang/trackerslist source.
    /// </summary>
    /// <param name="httpClient">The caller-configured HTTP client.</param>
    /// <param name="baseAddress">
    /// The raw repository base address.
    /// </param>
    public NgosangTrackerSource(
        HttpClient httpClient,
        Uri? baseAddress = null)
        : base(
            httpClient,
            baseAddress ?? DefaultBaseAddress,
            TorrentTrackerSourceKind.Ngosang)
    {
    }

    /// <inheritdoc />
    public override Task<TorrentTrackerList> GetAsync(
        TorrentTrackerListKind listKind = TorrentTrackerListKind.Best,
        CancellationToken cancellationToken = default)
    {
        var path = listKind switch
        {
            TorrentTrackerListKind.Best => "trackers_best.txt",
            TorrentTrackerListKind.All => "trackers_all.txt",
            TorrentTrackerListKind.Udp => "trackers_all_udp.txt",
            TorrentTrackerListKind.Http => "trackers_all_http.txt",
            TorrentTrackerListKind.Https => "trackers_all_https.txt",
            TorrentTrackerListKind.WebSocket => "trackers_all_ws.txt",
            TorrentTrackerListKind.I2p => "trackers_all_i2p.txt",
            TorrentTrackerListKind.Yggdrasil => "trackers_all_yggdrasil.txt",
            _ => throw new NotSupportedException(
                $"ngosang/trackerslist does not expose a {listKind} list through this adapter."),
        };

        return this.GetTextListAsync(listKind, path, cancellationToken);
    }
}
