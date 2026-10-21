using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace Novolis.Transports.Torrent.Search;

/// <summary>
/// Searches the public Magnetz metadata API.
/// </summary>
/// <remarks>
/// Magnetz supplies searchable metadata and swarm health. It is separate from
/// the maintained tracker-list sources, which are used by the product host to
/// enrich magnets for peer discovery.
/// </remarks>
public sealed class MagnetzSearchProvider : ITorrentSearchProvider
{
    private const int PageSize = 25;
    private const int MaximumPage = 4;
    private readonly Uri endpoint;
    private readonly HttpClient httpClient;

    /// <summary>
    /// Creates a provider for the public Magnetz API.
    /// </summary>
    /// <param name="httpClient">The caller-configured HTTP client.</param>
    /// <param name="endpoint">
    /// An optional endpoint override, primarily useful for compatible services
    /// and tests.
    /// </param>
    public MagnetzSearchProvider(
        HttpClient httpClient,
        Uri? endpoint = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        this.endpoint = endpoint
            ?? new Uri("https://magnetz.eu/api/magnets/search");
        if (!this.endpoint.IsAbsoluteUri
            || this.endpoint.Scheme is not ("http" or "https"))
        {
            throw new ArgumentException(
                "The Magnetz endpoint must be an absolute HTTP or HTTPS URI.",
                nameof(endpoint));
        }

        this.httpClient = httpClient;
    }

    /// <inheritdoc />
    public string Name => "Magnetz";

    /// <summary>
    /// Gets the configured public-search endpoint.
    /// </summary>
    public Uri Endpoint => this.endpoint;

    /// <inheritdoc />
    public async Task<IReadOnlyList<TorrentSearchResult>> SearchAsync(
        TorrentSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        query.Validate();

        var firstPage = (query.Offset / PageSize) + 1;
        if (firstPage > MaximumPage)
            return [];

        var skip = query.Offset % PageSize;
        var pagesNeeded = Math.Max(
            1,
            (skip + query.Limit + PageSize - 1) / PageSize);
        var lastPage = Math.Min(
            MaximumPage,
            firstPage + pagesNeeded - 1);
        var results = new List<TorrentSearchResult>();

        for (var page = firstPage; page <= lastPage; page++)
        {
            using var response = await this.httpClient.GetAsync(
                    BuildRequestUri(query.Term, page),
                    cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(
                    stream,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            results.AddRange(ParseResults(document.RootElement));
            if (results.Count == 0
                || !HasData(document.RootElement))
            {
                break;
            }
        }

        return results
            .Skip(skip)
            .Take(query.Limit)
            .ToArray();
    }

    private Uri BuildRequestUri(string term, int page)
    {
        var builder = new UriBuilder(this.endpoint);
        var parameters = new List<string>();
        if (!string.IsNullOrWhiteSpace(builder.Query))
            parameters.Add(builder.Query.TrimStart('?'));

        parameters.Add("query=" + Uri.EscapeDataString(term));
        parameters.Add(
            "page=" + page.ToString(CultureInfo.InvariantCulture));
        builder.Query = string.Join("&", parameters);
        return builder.Uri;
    }

    private static IReadOnlyList<TorrentSearchResult> ParseResults(
        JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Magnetz returned a non-object response.");

        if (root.TryGetProperty("message", out var message)
            && message.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(message.GetString()))
        {
            throw new InvalidOperationException(
                $"Magnetz error: {message.GetString()}");
        }

        if (!root.TryGetProperty("data", out var data)
            || data.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException(
                "Magnetz returned no searchable data array.");
        }

        var results = new List<TorrentSearchResult>();
        foreach (var item in data.EnumerateArray())
        {
            var name = GetString(item, "name");
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var magnet = TryCreateUri(GetString(item, "magnet_link"));
            var attributes = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
            AddAttribute(item, attributes, "sqid");
            AddAttribute(item, attributes, "score");
            AddAttribute(item, attributes, "is_verified");
            AddAttribute(item, attributes, "web_url");

            results.Add(
                new TorrentSearchResult(name.Trim())
                {
                    MagnetUri = magnet,
                    InfoHash = GetString(item, "info_hash"),
                    SizeBytes = GetInt64(item, "size"),
                    Seeders = GetInt32(item, "seeders"),
                    Leechers = GetInt32(item, "leechers"),
                    Availability = GetDecimal(item, "health"),
                    PublishedAt = GetDate(item, "created_at"),
                    Source = "Magnetz",
                    Attributes = new ReadOnlyDictionary<string, string>(
                        attributes),
                });
        }

        return results;
    }

    private static bool HasData(JsonElement root) =>
        root.TryGetProperty("data", out var data)
        && data.ValueKind == JsonValueKind.Array
        && data.GetArrayLength() > 0;

    private static Uri? TryCreateUri(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && string.Equals(
                uri.Scheme,
                "magnet",
                StringComparison.OrdinalIgnoreCase)
            ? uri
            : null;

    private static string? GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int? GetInt32(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value)
            && value.TryGetInt32(out var number)
            ? number
            : null;

    private static long? GetInt64(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value)
            && value.TryGetInt64(out var number)
            ? number
            : null;

    private static decimal? GetDecimal(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value)
            && value.TryGetDecimal(out var number)
            ? number
            : null;

    private static DateTimeOffset? GetDate(
        JsonElement element,
        string name)
    {
        var value = GetString(element, name);
        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var date)
            ? date
            : null;
    }

    private static void AddAttribute(
        JsonElement element,
        IDictionary<string, string> attributes,
        string name)
    {
        if (!element.TryGetProperty(name, out var value)
            || value.ValueKind is not (
                JsonValueKind.String
                or JsonValueKind.Number
                or JsonValueKind.True
                or JsonValueKind.False))
        {
            return;
        }

        var text = value.ToString();
        if (!string.IsNullOrWhiteSpace(text))
            attributes[name] = text;
    }
}
