using System.Security.Cryptography;

namespace Novolis.Transports.Kad;

/// <summary>
/// A 128-bit eMule Kad node identifier.
/// </summary>
public readonly struct KadId : IEquatable<KadId>
{
    private readonly byte[] value;

    public KadId(ReadOnlySpan<byte> value)
    {
        if (value.Length != 16)
            throw new ArgumentException("A Kad id must contain 16 bytes.", nameof(value));
        this.value = value.ToArray();
    }

    public ReadOnlySpan<byte> Bytes => this.value ?? [];

    public static KadId CreateRandom()
    {
        var value = new byte[16];
        RandomNumberGenerator.Fill(value);
        return new KadId(value);
    }

    public static KadId Parse(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        return new KadId(Convert.FromHexString(text));
    }

    public byte[] ToArray() => this.value?.ToArray() ?? new byte[16];

    public byte[] Xor(KadId other)
    {
        var result = new byte[16];
        for (var index = 0; index < result.Length; index++)
            result[index] = (byte)(this.Bytes[index] ^ other.Bytes[index]);
        return result;
    }

    public int GetBucketIndex(KadId other)
    {
        for (var index = 0; index < 16; index++)
        {
            var difference = (byte)(this.Bytes[index] ^ other.Bytes[index]);
            if (difference == 0)
                continue;

            var leadingZeros = 0;
            for (var bit = 7; bit >= 0 && (difference & (1 << bit)) == 0; bit--)
                leadingZeros++;
            return index * 8 + leadingZeros;
        }

        return -1;
    }

    public int CompareDistance(KadId left, KadId right)
    {
        var leftDistance = Xor(left);
        var rightDistance = Xor(right);
        for (var index = 0; index < leftDistance.Length; index++)
        {
            var comparison = leftDistance[index].CompareTo(rightDistance[index]);
            if (comparison != 0)
                return comparison;
        }

        return 0;
    }

    public bool Equals(KadId other) =>
        CryptographicOperations.FixedTimeEquals(this.Bytes, other.Bytes);

    public override bool Equals(object? obj) => obj is KadId other && Equals(other);

    public override int GetHashCode()
    {
        var bytes = this.Bytes;
        return HashCode.Combine(
            BitConverter.ToInt32(bytes[..4]),
            BitConverter.ToInt32(bytes[4..8]),
            BitConverter.ToInt32(bytes[8..12]),
            BitConverter.ToInt32(bytes[12..16]));
    }

    public override string ToString() => Convert.ToHexString(this.Bytes).ToLowerInvariant();

    public static bool operator ==(KadId left, KadId right) => left.Equals(right);

    public static bool operator !=(KadId left, KadId right) => !left.Equals(right);
}
