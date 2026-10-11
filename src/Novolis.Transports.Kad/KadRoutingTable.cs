namespace Novolis.Transports.Kad;

/// <summary>
/// The local node's k-bucket routing table.
/// </summary>
public sealed class KadRoutingTable
{
    public const int Alpha = 3;
    public const int BucketCount = 128;
    public const int BucketSize = 10;

    private readonly List<KadContact>[] buckets = Enumerable
        .Range(0, BucketCount)
        .Select(_ => new List<KadContact>(BucketSize))
        .ToArray();
    private readonly object sync = new();

    public KadRoutingTable(KadId localId)
    {
        this.LocalId = localId;
    }

    public KadId LocalId { get; }

    public bool TryAdd(KadContact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);
        var bucketIndex = this.LocalId.GetBucketIndex(contact.Id);
        if (bucketIndex < 0)
            return false;

        lock (this.sync)
        {
            var bucket = this.buckets[bucketIndex];
            var existing = bucket.FindIndex(item => item.Id == contact.Id);
            if (existing >= 0)
            {
                bucket.RemoveAt(existing);
                bucket.Add(contact);
                return true;
            }

            if (bucket.Count >= BucketSize)
                return false;

            bucket.Add(contact);
            return true;
        }
    }

    public IReadOnlyList<KadContact> GetBucket(int index)
    {
        if (index is < 0 or >= BucketCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        lock (this.sync)
        {
            return this.buckets[index].ToArray();
        }
    }

    public IReadOnlyList<KadContact> FindClosest(KadId target, int count = BucketSize)
    {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count));
        lock (this.sync)
        {
            return this.buckets
                .SelectMany(bucket => bucket)
                .OrderBy(contact => this.LocalId.CompareDistance(contact.Id, target))
                .Take(count)
                .ToArray();
        }
    }
}
