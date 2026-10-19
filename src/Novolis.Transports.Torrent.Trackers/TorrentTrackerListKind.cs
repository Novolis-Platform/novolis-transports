namespace Novolis.Transports.Torrent.Trackers;

/// <summary>
/// Selects a tracker list exposed by one or more tracker-list services.
/// </summary>
public enum TorrentTrackerListKind
{
    /// <summary>
    /// A source's stable or best curated list.
    /// </summary>
    Best,

    /// <summary>
    /// A source's complete public list.
    /// </summary>
    All,

    /// <summary>
    /// Currently responding trackers, where the source supports that view.
    /// </summary>
    Live,

    /// <summary>
    /// Trackers with a source-reported stable uptime, where supported.
    /// </summary>
    Stable,

    /// <summary>
    /// UDP trackers.
    /// </summary>
    Udp,

    /// <summary>
    /// HTTP trackers.
    /// </summary>
    Http,

    /// <summary>
    /// HTTPS trackers.
    /// </summary>
    Https,

    /// <summary>
    /// WebSocket trackers.
    /// </summary>
    WebSocket,

    /// <summary>
    /// I2P trackers.
    /// </summary>
    I2p,

    /// <summary>
    /// Yggdrasil trackers.
    /// </summary>
    Yggdrasil,

    /// <summary>
    /// Trackers other than HTTP and HTTPS.
    /// </summary>
    NoHttp,
}
