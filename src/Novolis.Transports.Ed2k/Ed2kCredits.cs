namespace Novolis.Transports.Ed2k;

/// <summary>
/// Per-peer upload and download totals used by the eD2k queue.
/// </summary>
public sealed class Ed2kCredits
{
    private readonly Dictionary<string, Ed2kCreditRecord> records = new(StringComparer.Ordinal);
    private readonly object sync = new();

    public void RecordUpload(ReadOnlySpan<byte> peerIdentity, long bytes)
    {
        Record(peerIdentity, bytes, upload: true);
    }

    public void RecordDownload(ReadOnlySpan<byte> peerIdentity, long bytes)
    {
        Record(peerIdentity, bytes, upload: false);
    }

    public Ed2kCreditRecord GetRecord(ReadOnlySpan<byte> peerIdentity)
    {
        var key = Convert.ToHexString(peerIdentity);
        lock (this.sync)
        {
            return this.records.TryGetValue(key, out var record)
                ? record.Clone()
                : new Ed2kCreditRecord(0, 0);
        }
    }

    public double GetModifier(ReadOnlySpan<byte> peerIdentity)
    {
        return GetModifier(GetRecord(peerIdentity));
    }

    public double GetQueueScore(
        ReadOnlySpan<byte> peerIdentity,
        TimeSpan waiting,
        int filePriority = 1)
    {
        if (waiting < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(waiting));
        if (filePriority < 1)
            throw new ArgumentOutOfRangeException(nameof(filePriority));

        return waiting.TotalSeconds * GetModifier(peerIdentity) * filePriority;
    }

    internal static double GetModifier(Ed2kCreditRecord record)
    {
        const double megabyte = 1024d * 1024d;
        var uploaded = record.UploadedBytes / megabyte;
        var downloaded = record.DownloadedBytes / megabyte;
        if (downloaded < 1)
            return 1;

        var ratio = uploaded / downloaded;
        var squareRoot = Math.Sqrt(uploaded + 2);
        return Math.Clamp(Math.Min(ratio, squareRoot), 1, 10);
    }

    private void Record(ReadOnlySpan<byte> peerIdentity, long bytes, bool upload)
    {
        if (peerIdentity.Length != 16)
            throw new ArgumentException("A peer identity must contain 16 bytes.", nameof(peerIdentity));
        if (bytes < 0)
            throw new ArgumentOutOfRangeException(nameof(bytes));

        var key = Convert.ToHexString(peerIdentity);
        lock (this.sync)
        {
            if (!this.records.TryGetValue(key, out var record))
                this.records[key] = record = new Ed2kCreditRecord(0, 0);
            if (upload)
                record.UploadedBytes = checked(record.UploadedBytes + bytes);
            else
                record.DownloadedBytes = checked(record.DownloadedBytes + bytes);
        }
    }
}
