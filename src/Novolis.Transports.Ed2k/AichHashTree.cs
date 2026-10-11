using System.Security.Cryptography;

namespace Novolis.Transports.Ed2k;

/// <summary>
/// SHA-1 hash tree used by the eMule Advanced Intelligent Corruption Handler.
/// </summary>
public sealed class AichHashTree
{
    private AichHashTree(byte[] rootHash, IReadOnlyList<byte[]> blockHashes)
    {
        this.RootHash = rootHash;
        this.BlockHashes = blockHashes;
    }

    public byte[] RootHash { get; }

    public IReadOnlyList<byte[]> BlockHashes { get; }

    public static AichHashTree Compute(ReadOnlySpan<byte> data)
    {
        var leaves = new List<byte[]>();
        if (data.Length == 0)
            leaves.Add(SHA1.HashData([]));
        else
            for (var offset = 0; offset < data.Length; offset += Ed2kProtocol.AichBlockSize)
            {
                var count = Math.Min(Ed2kProtocol.AichBlockSize, data.Length - offset);
                leaves.Add(SHA1.HashData(data.Slice(offset, count)));
            }

        var level = leaves.Select(hash => hash.ToArray()).ToList();
        while (level.Count > 1)
        {
            var next = new List<byte[]>((level.Count + 1) / 2);
            for (var index = 0; index < level.Count; index += 2)
            {
                var right = index + 1 < level.Count ? level[index + 1] : level[index];
                var combined = new byte[level[index].Length + right.Length];
                level[index].CopyTo(combined, 0);
                right.CopyTo(combined, level[index].Length);
                next.Add(SHA1.HashData(combined));
            }

            level = next;
        }

        return new AichHashTree(level[0], leaves);
    }

    public bool VerifyBlock(int index, ReadOnlySpan<byte> block)
    {
        if (index < 0 || index >= this.BlockHashes.Count)
            return false;
        return CryptographicOperations.FixedTimeEquals(
            this.BlockHashes[index],
            SHA1.HashData(block));
    }

    public bool VerifyFile(ReadOnlySpan<byte> data)
    {
        var tree = Compute(data);
        return CryptographicOperations.FixedTimeEquals(this.RootHash, tree.RootHash);
    }
}
