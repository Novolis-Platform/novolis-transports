namespace Novolis.Transports.Torrent.Trackers;

/// <summary>
/// Common HTTP implementation for text-based tracker-list services.
/// </summary>
public abstract class TorrentTrackerSourceBase : ITorrentTrackerSource
{
    /// <summary>
    /// Creates an HTTP tracker-list source.
    /// </summary>
    protected TorrentTrackerSourceBase(
        HttpClient httpClient,
        Uri baseAddress,
        TorrentTrackerSourceKind kind)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(baseAddress);
        if (!baseAddress.IsAbsoluteUri
            || baseAddress.Scheme is not ("http" or "https"))
        {
            throw new ArgumentException(
                "The tracker-list base address must be an absolute HTTP or HTTPS URI.",
                nameof(baseAddress));
        }

        this.HttpClient = httpClient;
        this.BaseAddress = new Uri(
            baseAddress.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
                ? baseAddress.AbsoluteUri
                : baseAddress.AbsoluteUri + "/",
            UriKind.Absolute);
        this.Kind = kind;
    }

    /// <summary>
    /// Gets the service identity.
    /// </summary>
    public TorrentTrackerSourceKind Kind { get; }

    /// <summary>
    /// Retrieves a list from the service.
    /// </summary>
    public abstract Task<TorrentTrackerList> GetAsync(
        TorrentTrackerListKind listKind = TorrentTrackerListKind.Best,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the HTTP client supplied by the caller.
    /// </summary>
    protected HttpClient HttpClient { get; }

    /// <summary>
    /// Gets the source base address.
    /// </summary>
    protected Uri BaseAddress { get; }

    /// <summary>
    /// Downloads and parses one newline-delimited tracker list.
    /// </summary>
    protected async Task<TorrentTrackerList> GetTextListAsync(
        TorrentTrackerListKind listKind,
        string relativePath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        var endpoint = new Uri(this.BaseAddress, relativePath);
        using var response = await this.HttpClient.GetAsync(
                endpoint,
                cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var text = await response.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);
        return TorrentTrackerTextParser.Parse(
            this.Kind,
            listKind,
            endpoint,
            text);
    }
}
