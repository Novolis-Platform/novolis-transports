using System.Security.Cryptography;

namespace Novolis.Transports.Ed2k;

/// <summary>
/// The 128-bit MD4 file identifier used by the eD2k protocol.
/// </summary>
public readonly struct Ed2kFileId : IEquatable<Ed2kFileId>
{
    private readonly byte[] value;

    public Ed2kFileId(ReadOnlySpan<byte> value)
    {
        if (value.Length != 16)
            throw new ArgumentException("An eD2k file id must contain 16 bytes.", nameof(value));

        this.value = value.ToArray();
    }

    public ReadOnlySpan<byte> Bytes => this.value ?? [];

    public static Ed2kFileId Compute(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return Compute(data.AsSpan());
    }

    public static Ed2kFileId Compute(ReadOnlySpan<byte> data)
    {
        if (data.Length <= Ed2kProtocol.PartSize)
            return new Ed2kFileId(Ed2kMd4.Compute(data));

        var partHashes = new List<byte[]>();
        var offset = 0;
        while (offset < data.Length)
        {
            var count = Math.Min(Ed2kProtocol.PartSize, data.Length - offset);
            partHashes.Add(Ed2kMd4.Compute(data.Slice(offset, count)));
            offset += count;
        }

        return new Ed2kFileId(Ed2kMd4.Compute(Flatten(partHashes)));
    }

    public static Ed2kFileId Compute(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var partHashes = ReadPartHashes(stream);
        return FromPartHashes(partHashes);
    }

    public static async ValueTask<Ed2kFileId> ComputeAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var partHashes = new List<byte[]>();
        var buffer = new byte[Ed2kProtocol.PartSize];

        while (true)
        {
            var length = 0;
            while (length < buffer.Length)
            {
                var read = await stream.ReadAsync(
                        buffer.AsMemory(length, buffer.Length - length),
                        cancellationToken)
                    .ConfigureAwait(false);
                if (read == 0)
                    break;
                length += read;
            }

            if (length == 0)
                break;

            partHashes.Add(Ed2kMd4.Compute(buffer.AsSpan(0, length)));
            if (length < buffer.Length)
                break;
        }

        return FromPartHashes(partHashes);
    }

    public static Ed2kFileId Parse(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        try
        {
            return new Ed2kFileId(Convert.FromHexString(text));
        }
        catch (FormatException exception)
        {
            throw new FormatException("An eD2k file id must be 32 hexadecimal characters.", exception);
        }
    }

    public byte[] ToArray() => this.value?.ToArray() ?? new byte[16];

    public bool Equals(Ed2kFileId other)
    {
        return CryptographicOperations.FixedTimeEquals(this.Bytes, other.Bytes);
    }

    public override bool Equals(object? obj) => obj is Ed2kFileId other && Equals(other);

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

    public static bool operator ==(Ed2kFileId left, Ed2kFileId right) => left.Equals(right);

    public static bool operator !=(Ed2kFileId left, Ed2kFileId right) => !left.Equals(right);

    private static List<byte[]> ReadPartHashes(Stream stream)
    {
        var partHashes = new List<byte[]>();
        var buffer = new byte[Ed2kProtocol.PartSize];
        while (true)
        {
            var length = 0;
            while (length < buffer.Length)
            {
                var read = stream.Read(buffer, length, buffer.Length - length);
                if (read == 0)
                    break;
                length += read;
            }

            if (length == 0)
                break;

            partHashes.Add(Ed2kMd4.Compute(buffer.AsSpan(0, length)));
            if (length < buffer.Length)
                break;
        }

        return partHashes;
    }

    private static Ed2kFileId FromPartHashes(List<byte[]> partHashes)
    {
        if (partHashes.Count == 0)
            return new Ed2kFileId(Ed2kMd4.Compute([]));
        if (partHashes.Count == 1)
            return new Ed2kFileId(partHashes[0]);
        return new Ed2kFileId(Ed2kMd4.Compute(Flatten(partHashes)));
    }

    private static byte[] Flatten(List<byte[]> values)
    {
        var result = new byte[checked(values.Count * 16)];
        for (var index = 0; index < values.Count; index++)
            values[index].CopyTo(result, index * 16);
        return result;
    }
}
