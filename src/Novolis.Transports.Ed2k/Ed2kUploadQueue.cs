namespace Novolis.Transports.Ed2k;

/// <summary>
/// Pairwise-credit-aware upload queue.
/// </summary>
public sealed class Ed2kUploadQueue
{
    private readonly Ed2kCredits credits;
    private readonly List<Ed2kUploadQueueEntry> entries = new();
    private readonly object sync = new();

    public Ed2kUploadQueue(Ed2kCredits credits)
    {
        this.credits = credits ?? throw new ArgumentNullException(nameof(credits));
    }

    public int Count
    {
        get
        {
            lock (this.sync)
                return this.entries.Count;
        }
    }

    public void Enqueue(
        ReadOnlySpan<byte> peerIdentity,
        int filePriority = 1,
        DateTimeOffset? queuedAt = null)
    {
        if (peerIdentity.Length != 16)
            throw new ArgumentException("A peer identity must contain 16 bytes.", nameof(peerIdentity));
        if (filePriority < 1)
            throw new ArgumentOutOfRangeException(nameof(filePriority));

        var entry = new Ed2kUploadQueueEntry(
            peerIdentity.ToArray(),
            filePriority,
            queuedAt ?? DateTimeOffset.UtcNow);
        lock (this.sync)
            this.entries.Add(entry);
    }

    public bool TryTakeNext(DateTimeOffset now, out Ed2kUploadQueueEntry? entry)
    {
        lock (this.sync)
        {
            if (this.entries.Count == 0)
            {
                entry = null;
                return false;
            }

            var best = this.entries
                .OrderByDescending(item =>
                    this.credits.GetQueueScore(
                        item.PeerIdentity,
                        now - item.QueuedAt,
                        item.FilePriority))
                .First();
            this.entries.Remove(best);
            entry = best;
            return true;
        }
    }
}
