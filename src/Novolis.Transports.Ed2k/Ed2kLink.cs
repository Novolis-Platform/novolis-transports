namespace Novolis.Transports.Ed2k;

/// <summary>
/// A standard ed2k file or server link.
/// </summary>
public sealed class Ed2kLink
{
    private Ed2kLink(
        Ed2kLinkKind kind,
        string? fileName,
        long size,
        Ed2kFileId fileId,
        Ed2kServerEndpoint? server)
    {
        this.Kind = kind;
        this.FileName = fileName;
        this.Size = size;
        this.FileId = fileId;
        this.Server = server;
    }

    public Ed2kLinkKind Kind { get; }

    public string? FileName { get; }

    public long Size { get; }

    public Ed2kFileId FileId { get; }

    public Ed2kServerEndpoint? Server { get; }

    public static Ed2kLink CreateFile(string fileName, long size, Ed2kFileId fileId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentOutOfRangeException.ThrowIfNegative(size);
        return new Ed2kLink(Ed2kLinkKind.File, fileName, size, fileId, null);
    }

    public static Ed2kLink CreateServer(Ed2kServerEndpoint server)
    {
        ArgumentNullException.ThrowIfNull(server);
        if (server.Port is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(server));
        return new Ed2kLink(Ed2kLinkKind.Server, null, 0, default, server);
    }

    public static Ed2kLink Parse(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        var parts = text.Split('|');
        if (parts.Length < 4 ||
            !parts[0].Equals("ed2k://", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("The link is not an ed2k link.");

        if (parts[1].Equals("file", StringComparison.OrdinalIgnoreCase))
        {
            if (parts.Length < 6 ||
                !long.TryParse(parts[3], out var size) ||
                size < 0)
                throw new FormatException("The ed2k file link has an invalid size.");

            return CreateFile(
                Uri.UnescapeDataString(parts[2]),
                size,
                Ed2kFileId.Parse(parts[4]));
        }

        if (parts[1].Equals("server", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(parts[3], out var port))
        {
            return CreateServer(new Ed2kServerEndpoint(parts[2], port));
        }

        throw new FormatException("The ed2k link kind is not supported.");
    }

    public static bool TryParse(string? text, out Ed2kLink? link)
    {
        link = null;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        try
        {
            link = Parse(text);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public override string ToString()
    {
        return this.Kind switch
        {
            Ed2kLinkKind.File =>
                $"ed2k://|file|{Uri.EscapeDataString(this.FileName!)}|{this.Size}|{this.FileId}|/",
            Ed2kLinkKind.Server =>
                $"ed2k://|server|{this.Server!.Host}|{this.Server.Port}|/",
            _ => throw new InvalidOperationException()
        };
    }
}
