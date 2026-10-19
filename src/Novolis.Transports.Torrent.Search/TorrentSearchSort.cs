namespace Novolis.Transports.Torrent.Search;

/// <summary>
/// Sort orders supported by the provider-neutral search client.
/// </summary>
public enum TorrentSearchSort
{
    /// <summary>
    /// Preserve provider order.
    /// </summary>
    Relevance,

    /// <summary>
    /// Sort by seeder count.
    /// </summary>
    Seeders,

    /// <summary>
    /// Sort by leecher count.
    /// </summary>
    Leechers,

    /// <summary>
    /// Sort by effective ratio.
    /// </summary>
    Ratio,

    /// <summary>
    /// Sort by availability.
    /// </summary>
    Availability,

    /// <summary>
    /// Sort by size in bytes.
    /// </summary>
    Size,

    /// <summary>
    /// Sort by publication time.
    /// </summary>
    PublishedAt,
}
