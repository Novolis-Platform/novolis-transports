namespace Novolis.Transports.Torrent.Trackers;

/// <summary>
/// Identifies a public tracker-list service.
/// </summary>
public enum TorrentTrackerSourceKind
{
    /// <summary>
    /// The newTrackon tracker health service.
    /// </summary>
    NewTrackon,

    /// <summary>
    /// The ngosang/trackerslist daily lists.
    /// </summary>
    Ngosang,

    /// <summary>
    /// The XIU2 TrackersListCollection daily lists.
    /// </summary>
    Xiu2TrackersListCollection,
}
