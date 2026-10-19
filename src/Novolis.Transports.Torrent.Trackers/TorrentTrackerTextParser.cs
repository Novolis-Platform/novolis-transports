namespace Novolis.Transports.Torrent.Trackers;

internal static class TorrentTrackerTextParser
{
    private static readonly HashSet<string> SupportedSchemes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "http",
            "https",
            "udp",
            "ws",
            "wss",
        };

    public static TorrentTrackerList Parse(
        TorrentTrackerSourceKind source,
        TorrentTrackerListKind kind,
        Uri endpoint,
        string text)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(text);

        var trackers = new List<Uri>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in text.Split(
                     ["\r\n", "\n", "\r"],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            var value = line.Trim();
            if (value.Length == 0
                || value.StartsWith("#", StringComparison.Ordinal)
                || value.StartsWith("//", StringComparison.Ordinal)
                || !Uri.TryCreate(value, UriKind.Absolute, out var tracker)
                || !SupportedSchemes.Contains(tracker.Scheme)
                || !seen.Add(tracker.AbsoluteUri))
            {
                continue;
            }

            trackers.Add(tracker);
        }

        return new TorrentTrackerList(
            source,
            kind,
            endpoint,
            DateTimeOffset.UtcNow,
            trackers);
    }
}
