namespace Novolis.Transports.Ed2k;

/// <summary>
/// A mutable credit record returned as a snapshot by <see cref="Ed2kCredits" />.
/// </summary>
public sealed class Ed2kCreditRecord
{
    internal Ed2kCreditRecord(long uploadedBytes, long downloadedBytes)
    {
        this.UploadedBytes = uploadedBytes;
        this.DownloadedBytes = downloadedBytes;
    }

    public long UploadedBytes { get; internal set; }

    public long DownloadedBytes { get; internal set; }

    internal Ed2kCreditRecord Clone() => new(this.UploadedBytes, this.DownloadedBytes);
}
