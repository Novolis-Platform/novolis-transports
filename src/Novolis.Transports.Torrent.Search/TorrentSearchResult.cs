namespace Novolis.Transports.Torrent.Search;

/// <summary>
/// A normalized result returned by a torrent search provider.
/// </summary>
public sealed record TorrentSearchResult
{
    /// <summary>
    /// Creates a search result.
    /// </summary>
    public TorrentSearchResult(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        this.Name = name;
    }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the magnet URI, when supplied by the provider.
    /// </summary>
    public Uri? MagnetUri { get; init; }

    /// <summary>
    /// Gets the downloadable torrent URI, when supplied by the provider.
    /// </summary>
    public Uri? TorrentUri { get; init; }

    /// <summary>
    /// Gets the BitTorrent info hash, when supplied or parsed.
    /// </summary>
    public string? InfoHash { get; init; }

    /// <summary>
    /// Gets the content size in bytes.
    /// </summary>
    public long? SizeBytes { get; init; }

    /// <summary>
    /// Gets the number of complete sources.
    /// </summary>
    public int? Seeders { get; init; }

    /// <summary>
    /// Gets the number of incomplete sources.
    /// </summary>
    public int? Leechers { get; init; }

    /// <summary>
    /// Gets a provider-supplied ratio, when available.
    /// </summary>
    public decimal? Ratio { get; init; }

    /// <summary>
    /// Gets the source-provided availability value, when available.
    /// </summary>
    public decimal? Availability { get; init; }

    /// <summary>
    /// Gets the number of completed downloads, when available.
    /// </summary>
    public long? CompletedDownloads { get; init; }

    /// <summary>
    /// Gets the publication timestamp, when available.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; init; }

    /// <summary>
    /// Gets the provider category.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets the provider identity.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Gets provider-specific attributes that were not normalized.
    /// </summary>
    public IReadOnlyDictionary<string, string> Attributes { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the complete-source to incomplete-source ratio.
    /// </summary>
    public decimal? SeederLeecherRatio
    {
        get
        {
            if (!this.Seeders.HasValue || !this.Leechers.HasValue)
                return null;
            if (this.Leechers.Value == 0)
                return this.Seeders.Value == 0 ? 0m : decimal.MaxValue;

            return (decimal)this.Seeders.Value / this.Leechers.Value;
        }
    }

    /// <summary>
    /// Gets the provider ratio, falling back to the seeder-to-leecher ratio.
    /// </summary>
    public decimal? EffectiveRatio => this.Ratio ?? this.SeederLeecherRatio;
}
