using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml.Linq;

namespace Novolis.Transports.Torrent.Search;

/// <summary>
/// Searches any Torznab-compatible indexer or indexer aggregator.
/// </summary>
/// <remarks>
/// Torznab is an adapter boundary rather than an indexer catalogue. Point it
/// at a configured Jackett, Prowlarr, or other Torznab endpoint and keep the
/// endpoint's authentication in the caller's configuration.
/// </remarks>
public sealed class TorznabSearchProvider : ITorrentSearchProvider
{
    private readonly HttpClient httpClient;
    private readonly Uri endpoint;
    private readonly string? apiKey;

    /// <summary>
    /// Creates a Torznab search provider.
    /// </summary>
    /// <param name="httpClient">The caller-configured HTTP client.</param>
    /// <param name="endpoint">The Torznab API endpoint.</param>
    /// <param name="apiKey">An optional Torznab API key.</param>
    /// <param name="name">An optional display name for the provider.</param>
    public TorznabSearchProvider(
        HttpClient httpClient,
        Uri endpoint,
        string? apiKey = null,
        string? name = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(endpoint);
        if (!endpoint.IsAbsoluteUri
            || endpoint.Scheme is not ("http" or "https"))
        {
            throw new ArgumentException(
                "The Torznab endpoint must be an absolute HTTP or HTTPS URI.",
                nameof(endpoint));
        }

        this.httpClient = httpClient;
        this.endpoint = endpoint;
        this.apiKey = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey;
        this.Name = string.IsNullOrWhiteSpace(name)
            ? endpoint.Host
            : name.Trim();
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Gets the configured Torznab endpoint.
    /// </summary>
    public Uri Endpoint => this.endpoint;

    /// <inheritdoc />
    public async Task<IReadOnlyList<TorrentSearchResult>> SearchAsync(
        TorrentSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        query.Validate();

        using var response = await this.httpClient.GetAsync(
                BuildRequestUri(query),
                cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);
        var results = ParseResults(xml);
        return TorrentSearchClient.Apply(query, results);
    }

    private Uri BuildRequestUri(TorrentSearchQuery query)
    {
        var builder = new UriBuilder(this.endpoint);
        var parameters = new List<string>();
        if (!string.IsNullOrWhiteSpace(builder.Query))
            parameters.Add(builder.Query.TrimStart('?'));

        parameters.Add("t=search");
        parameters.Add("q=" + Uri.EscapeDataString(query.Term));
        parameters.Add("limit=" + query.Limit.ToString(CultureInfo.InvariantCulture));
        parameters.Add("offset=" + query.Offset.ToString(CultureInfo.InvariantCulture));
        parameters.Add("extended=1");
        if (!string.IsNullOrWhiteSpace(query.Category))
            parameters.Add("cat=" + Uri.EscapeDataString(query.Category));
        if (this.apiKey is not null)
            parameters.Add("apikey=" + Uri.EscapeDataString(this.apiKey));

        builder.Query = string.Join("&", parameters);
        return builder.Uri;
    }

    private IReadOnlyList<TorrentSearchResult> ParseResults(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);
        var document = XDocument.Parse(xml, LoadOptions.None);
        var error = document
            .Descendants()
            .FirstOrDefault(element =>
                string.Equals(
                    element.Name.LocalName,
                    "error",
                    StringComparison.OrdinalIgnoreCase));
        if (error is not null)
        {
            var code = error.Attribute("code")?.Value;
            var description = error.Attribute("description")?.Value
                              ?? "The Torznab provider returned an error.";
            throw new InvalidOperationException(
                code is null
                    ? description
                    : $"Torznab error {code}: {description}");
        }

        var results = new List<TorrentSearchResult>();
        foreach (var item in document
                     .Descendants()
                     .Where(element =>
                         string.Equals(
                             element.Name.LocalName,
                             "item",
                             StringComparison.OrdinalIgnoreCase)))
        {
            var name = GetElementValue(item, "title");
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var attributes = ReadAttributes(item);
            var link = GetElementValue(item, "link");
            var guid = GetElementValue(item, "guid");
            var enclosure = item
                .Elements()
                .FirstOrDefault(element =>
                    string.Equals(
                        element.Name.LocalName,
                        "enclosure",
                        StringComparison.OrdinalIgnoreCase));
            var enclosureUri = enclosure?.Attribute("url")?.Value;
            var magnetUri = FirstUri(
                GetAttribute(attributes, "magneturl"),
                link,
                guid,
                onlyScheme: "magnet");
            var torrentUri = FirstUri(
                enclosureUri,
                link,
                guid,
                onlyScheme: null,
                excludedScheme: "magnet");

            var result = new TorrentSearchResult(name.Trim())
            {
                MagnetUri = magnetUri,
                TorrentUri = torrentUri,
                InfoHash = NormalizeInfoHash(
                    GetAttribute(attributes, "infohash")
                    ?? ParseMagnetInfoHash(magnetUri)),
                SizeBytes = FirstLong(
                    GetAttribute(attributes, "size"),
                    enclosure?.Attribute("length")?.Value,
                    GetElementValue(item, "size")),
                Seeders = FirstInt(
                    GetAttribute(attributes, "seeders"),
                    GetAttribute(attributes, "complete")),
                Leechers = FirstInt(
                    GetAttribute(attributes, "leechers"),
                    GetAttribute(attributes, "incomplete")),
                Ratio = FirstDecimal(GetAttribute(attributes, "ratio")),
                Availability = FirstDecimal(
                    GetAttribute(attributes, "availability"),
                    GetAttribute(attributes, "availabilitypercent"),
                    GetAttribute(attributes, "available")),
                CompletedDownloads = FirstLong(
                    GetAttribute(attributes, "grabs"),
                    GetAttribute(attributes, "completed")),
                PublishedAt = ParseDate(
                    GetElementValue(item, "pubDate")
                    ?? GetAttribute(attributes, "published")),
                Category = GetElementValue(item, "category"),
                Source = this.Name,
                Attributes = new ReadOnlyDictionary<string, string>(attributes),
            };
            results.Add(result);
        }

        return results;
    }

    private static Dictionary<string, string> ReadAttributes(XElement item)
    {
        var attributes = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var element in item.Elements())
        {
            if (!string.Equals(
                    element.Name.LocalName,
                    "attr",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var name = element.Attribute("name")?.Value;
            var value = element.Attribute("value")?.Value;
            if (!string.IsNullOrWhiteSpace(name) && value is not null)
                attributes[name] = value;
        }

        return attributes;
    }

    private static string? GetAttribute(
        IReadOnlyDictionary<string, string> attributes,
        string name)
    {
        return attributes.TryGetValue(name, out var value) ? value : null;
    }

    private static string? GetElementValue(XElement parent, string localName)
    {
        return parent
            .Elements()
            .FirstOrDefault(element =>
                string.Equals(
                    element.Name.LocalName,
                    localName,
                    StringComparison.OrdinalIgnoreCase))
            ?.Value
            .Trim();
    }

    private static Uri? FirstUri(
        string? first,
        string? second,
        string? third,
        string? onlyScheme,
        string? excludedScheme = null)
    {
        foreach (var value in new[] { first, second, third })
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
                continue;
            if (onlyScheme is not null
                && !string.Equals(
                    uri.Scheme,
                    onlyScheme,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            if (excludedScheme is not null
                && string.Equals(
                    uri.Scheme,
                    excludedScheme,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            if (onlyScheme is null
                && uri.Scheme is not ("http" or "https"))
            {
                continue;
            }

            return uri;
        }

        return null;
    }

    private static int? FirstInt(params string?[] values)
    {
        foreach (var value in values)
        {
            if (int.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var number))
            {
                return number;
            }
        }

        return null;
    }

    private static long? FirstLong(params string?[] values)
    {
        foreach (var value in values)
        {
            if (long.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var number))
            {
                return number;
            }
        }

        return null;
    }

    private static decimal? FirstDecimal(params string?[] values)
    {
        foreach (var value in values)
        {
            if (decimal.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var number))
            {
                return number;
            }
        }

        return null;
    }

    private static DateTimeOffset? ParseDate(string? value)
    {
        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var date)
            ? date
            : null;
    }

    private static string? ParseMagnetInfoHash(Uri? magnetUri)
    {
        if (magnetUri is null)
            return null;

        foreach (var parameter in magnetUri.Query.TrimStart('?').Split('&'))
        {
            var parts = parameter.Split('=', 2);
            if (parts.Length != 2
                || !string.Equals(
                    parts[0],
                    "xt",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = Uri.UnescapeDataString(parts[1]);
            const string prefix = "urn:btih:";
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return value[prefix.Length..];
        }

        return null;
    }

    private static string? NormalizeInfoHash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
