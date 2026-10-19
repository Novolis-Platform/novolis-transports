namespace Novolis.Transports.Torrent.Search;

/// <summary>
/// Search text, provider hints, and client-side result filters.
/// </summary>
public sealed record TorrentSearchQuery
{
    /// <summary>
    /// Creates a torrent search query.
    /// </summary>
    /// <param name="term">The free-text search term.</param>
    public TorrentSearchQuery(string term)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(term);
        this.Term = term.Trim();
    }

    /// <summary>
    /// Gets the free-text search term.
    /// </summary>
    public string Term { get; }

    /// <summary>
    /// Gets an optional Torznab category expression.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets the minimum complete-source count.
    /// </summary>
    public int? MinimumSeeders { get; init; }

    /// <summary>
    /// Gets the maximum complete-source count.
    /// </summary>
    public int? MaximumSeeders { get; init; }

    /// <summary>
    /// Gets the minimum incomplete-source count.
    /// </summary>
    public int? MinimumLeechers { get; init; }

    /// <summary>
    /// Gets the maximum incomplete-source count.
    /// </summary>
    public int? MaximumLeechers { get; init; }

    /// <summary>
    /// Gets the minimum effective ratio.
    /// </summary>
    /// <remarks>
    /// The provider ratio is preferred; otherwise the ratio of seeders to
    /// leechers is used.
    /// </remarks>
    public decimal? MinimumRatio { get; init; }

    /// <summary>
    /// Gets the maximum effective ratio.
    /// </summary>
    public decimal? MaximumRatio { get; init; }

    /// <summary>
    /// Gets the minimum source-provided availability.
    /// </summary>
    public decimal? MinimumAvailability { get; init; }

    /// <summary>
    /// Gets the maximum source-provided availability.
    /// </summary>
    public decimal? MaximumAvailability { get; init; }

    /// <summary>
    /// Gets the minimum content size in bytes.
    /// </summary>
    public long? MinimumSizeBytes { get; init; }

    /// <summary>
    /// Gets the maximum content size in bytes.
    /// </summary>
    public long? MaximumSizeBytes { get; init; }

    /// <summary>
    /// Gets the provider page size and final result limit.
    /// </summary>
    public int Limit { get; init; } = 100;

    /// <summary>
    /// Gets the provider page offset.
    /// </summary>
    public int Offset { get; init; }

    /// <summary>
    /// Gets the result sort field.
    /// </summary>
    public TorrentSearchSort SortBy { get; init; }

    /// <summary>
    /// Gets a value indicating whether the selected sort is descending.
    /// </summary>
    public bool SortDescending { get; init; } = true;

    /// <summary>
    /// Validates this query's numeric ranges.
    /// </summary>
    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(this.Limit, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(this.Offset, 0);

        ValidateRange(this.MinimumSeeders, this.MaximumSeeders, "seeders");
        ValidateRange(this.MinimumLeechers, this.MaximumLeechers, "leechers");
        ValidateRange(this.MinimumRatio, this.MaximumRatio, "ratio");
        ValidateRange(
            this.MinimumAvailability,
            this.MaximumAvailability,
            "availability");
        ValidateRange(
            this.MinimumSizeBytes,
            this.MaximumSizeBytes,
            "size");

        if (this.MinimumSeeders < 0
            || this.MinimumLeechers < 0
            || this.MinimumSizeBytes < 0
            || this.MaximumSizeBytes < 0
            || this.MinimumRatio < 0
            || this.MaximumRatio < 0
            || this.MinimumAvailability < 0
            || this.MaximumAvailability < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(TorrentSearchQuery),
                "Search filters cannot be negative.");
        }
    }

    /// <summary>
    /// Checks whether a result satisfies every configured filter.
    /// </summary>
    public bool Matches(TorrentSearchResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Matches(result.Seeders, this.MinimumSeeders, this.MaximumSeeders)
               && Matches(
                   result.Leechers,
                   this.MinimumLeechers,
                   this.MaximumLeechers)
               && Matches(
                   result.EffectiveRatio,
                   this.MinimumRatio,
                   this.MaximumRatio)
               && Matches(
                   result.Availability,
                   this.MinimumAvailability,
                   this.MaximumAvailability)
               && Matches(
                   result.SizeBytes,
                   this.MinimumSizeBytes,
                   this.MaximumSizeBytes);
    }

    private static bool Matches(
        int? value,
        int? minimum,
        int? maximum)
    {
        if (minimum.HasValue && (!value.HasValue || value < minimum))
            return false;
        if (maximum.HasValue && (!value.HasValue || value > maximum))
            return false;
        return true;
    }

    private static bool Matches(
        long? value,
        long? minimum,
        long? maximum)
    {
        if (minimum.HasValue && (!value.HasValue || value < minimum))
            return false;
        if (maximum.HasValue && (!value.HasValue || value > maximum))
            return false;
        return true;
    }

    private static bool Matches(
        decimal? value,
        decimal? minimum,
        decimal? maximum)
    {
        if (minimum.HasValue && (!value.HasValue || value < minimum))
            return false;
        if (maximum.HasValue && (!value.HasValue || value > maximum))
            return false;
        return true;
    }

    private static void ValidateRange<T>(
        T? minimum,
        T? maximum,
        string name)
        where T : struct, IComparable<T>
    {
        if (minimum.HasValue
            && maximum.HasValue
            && minimum.Value.CompareTo(maximum.Value) > 0)
        {
            throw new ArgumentException(
                $"The minimum {name} cannot be greater than the maximum.",
                name);
        }
    }
}
