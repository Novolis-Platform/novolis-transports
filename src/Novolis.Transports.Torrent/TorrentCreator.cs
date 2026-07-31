using System.Security.Cryptography;
using Novolis.Transports.Torrent.BEncoding;
using Novolis.Transports.Torrent.Extensions;

namespace Novolis.Transports.Torrent;

/// <summary>
///     Creates single-file <c>.torrent</c> metadata (BEncode) for dogfood and tests.
/// </summary>
public static class TorrentCreator
{
    /// <summary>
    ///     Builds a single-file torrent for <paramref name="filePath"/> and writes it to <paramref name="torrentPath"/>.
    /// </summary>
    public static TorrentInfo CreateSingleFile(
        string filePath,
        string torrentPath,
        long pieceLength = 256 * 1024,
        IEnumerable<Uri>? announceUrls = null,
        string? comment = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(torrentPath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Payload file not found.", filePath);
        if (pieceLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(pieceLength));

        var announces = (announceUrls ?? DefaultAnnounceUrls()).ToList();
        if (announces.Count == 0)
            throw new ArgumentException("At least one announce URL is required.", nameof(announceUrls));

        var name = Path.GetFileName(filePath);
        var length = new FileInfo(filePath).Length;
        if (length <= 0)
            throw new InvalidOperationException("Payload file is empty.");

        var pieceBytes = new List<byte>();
        var buffer = new byte[pieceLength];
        using (var stream = File.OpenRead(filePath))
        {
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var hash = SHA1.HashData(buffer.AsSpan(0, read));
                pieceBytes.AddRange(hash);
            }
        }

        var info = new BEncodedDictionary
        {
            [new BEncodedString("piece length")] = new BEncodedNumber(pieceLength),
            [new BEncodedString("pieces")] = new BEncodedString(pieceBytes.ToArray()),
            [new BEncodedString("name")] = new BEncodedString(name),
            [new BEncodedString("length")] = new BEncodedNumber(length)
        };

        var announceList = new BEncodedList();
        foreach (var uri in announces.DistinctBy(u => u.AbsoluteUri))
        {
            announceList.Add(new BEncodedList { new BEncodedString(uri.AbsoluteUri) });
        }

        var root = new BEncodedDictionary
        {
            [new BEncodedString("announce")] = new BEncodedString(announces[0].AbsoluteUri),
            [new BEncodedString("announce-list")] = announceList,
            [new BEncodedString("creation date")] = new BEncodedNumber(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            [new BEncodedString("created by")] = new BEncodedString("Novolis.Transports.Torrent"),
            [new BEncodedString("comment")] = new BEncodedString(comment ?? $"Novolis dogfood torrent for {name}"),
            [new BEncodedString("encoding")] = new BEncodedString("UTF8"),
            [new BEncodedString("info")] = info
        };

        var encoded = root.Encode();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(torrentPath))!);
        File.WriteAllBytes(torrentPath, encoded);

        if (!TorrentInfo.TryLoad(encoded, out var torrent) || torrent is null)
            throw new InvalidOperationException("Created torrent failed to parse.");

        return torrent;
    }

    static IEnumerable<Uri> DefaultAnnounceUrls() =>
    [
        new Uri("udp://tracker.opentrackr.org:1337/announce"),
        new Uri("udp://open.stealth.si:80/announce"),
        // openbittorrent.com returns HTTP 403; opentrackr speaks HTTP on the same host/port as UDP.
        new Uri("http://tracker.opentrackr.org:1337/announce")
    ];
}
