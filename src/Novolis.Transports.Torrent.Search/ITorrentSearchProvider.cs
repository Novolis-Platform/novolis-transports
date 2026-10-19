namespace Novolis.Transports.Torrent.Search;

/// <summary>
/// Supplies normalized torrent search results.
/// </summary>
public interface ITorrentSearchProvider
{
    /// <summary>
    /// Gets the provider identity.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Searches the provider.
    /// </summary>
    Task<IReadOnlyList<TorrentSearchResult>> SearchAsync(
        TorrentSearchQuery query,
        CancellationToken cancellationToken = default);
}
