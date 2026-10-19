namespace Novolis.Transports.Torrent.Trackers;

/// <summary>
/// Retrieves a public BitTorrent tracker list from one service.
/// </summary>
public interface ITorrentTrackerSource
{
    /// <summary>
    /// Gets the service identity.
    /// </summary>
    TorrentTrackerSourceKind Kind { get; }

    /// <summary>
    /// Retrieves a list from the service.
    /// </summary>
    Task<TorrentTrackerList> GetAsync(
        TorrentTrackerListKind listKind = TorrentTrackerListKind.Best,
        CancellationToken cancellationToken = default);
}
