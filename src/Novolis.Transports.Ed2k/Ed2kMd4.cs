using System.Buffers.Binary;

namespace Novolis.Transports.Ed2k;

/// <summary>
/// Minimal MD4 implementation used only for the historical eD2k file id.
/// </summary>
public static class Ed2kMd4
{
    public static byte[] Compute(ReadOnlySpan<byte> data)
    {
        var paddedLength = checked(((data.Length + 9 + 63) / 64) * 64);
        var padded = new byte[paddedLength];
        data.CopyTo(padded);
        padded[data.Length] = 0x80;
        BinaryPrimitives.WriteUInt64LittleEndian(
            padded.AsSpan(paddedLength - 8, 8),
            checked((ulong)data.Length * 8));

        var a = 0x67452301u;
        var b = 0xEFCDAB89u;
        var c = 0x98BADCFEu;
        var d = 0x10325476u;
        var words = new uint[16];

        for (var offset = 0; offset < padded.Length; offset += 64)
        {
            for (var index = 0; index < words.Length; index++)
            {
                words[index] = BinaryPrimitives.ReadUInt32LittleEndian(
                    padded.AsSpan(offset + index * 4, 4));
            }

            var aa = a;
            var bb = b;
            var cc = c;
            var dd = d;

            RoundOne(ref a, ref b, ref c, ref d, words);
            RoundTwo(ref a, ref b, ref c, ref d, words);
            RoundThree(ref a, ref b, ref c, ref d, words);

            a += aa;
            b += bb;
            c += cc;
            d += dd;
        }

        var result = new byte[16];
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(0, 4), a);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4, 4), b);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(8, 4), c);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(12, 4), d);
        return result;
    }

    private static void RoundOne(
        ref uint a,
        ref uint b,
        ref uint c,
        ref uint d,
        uint[] x)
    {
        a = RotateLeft(a + F(b, c, d) + x[0], 3);
        d = RotateLeft(d + F(a, b, c) + x[1], 7);
        c = RotateLeft(c + F(d, a, b) + x[2], 11);
        b = RotateLeft(b + F(c, d, a) + x[3], 19);
        a = RotateLeft(a + F(b, c, d) + x[4], 3);
        d = RotateLeft(d + F(a, b, c) + x[5], 7);
        c = RotateLeft(c + F(d, a, b) + x[6], 11);
        b = RotateLeft(b + F(c, d, a) + x[7], 19);
        a = RotateLeft(a + F(b, c, d) + x[8], 3);
        d = RotateLeft(d + F(a, b, c) + x[9], 7);
        c = RotateLeft(c + F(d, a, b) + x[10], 11);
        b = RotateLeft(b + F(c, d, a) + x[11], 19);
        a = RotateLeft(a + F(b, c, d) + x[12], 3);
        d = RotateLeft(d + F(a, b, c) + x[13], 7);
        c = RotateLeft(c + F(d, a, b) + x[14], 11);
        b = RotateLeft(b + F(c, d, a) + x[15], 19);
    }

    private static void RoundTwo(
        ref uint a,
        ref uint b,
        ref uint c,
        ref uint d,
        uint[] x)
    {
        const uint k = 0x5A827999;
        a = RotateLeft(a + G(b, c, d) + x[0] + k, 3);
        d = RotateLeft(d + G(a, b, c) + x[4] + k, 5);
        c = RotateLeft(c + G(d, a, b) + x[8] + k, 9);
        b = RotateLeft(b + G(c, d, a) + x[12] + k, 13);
        a = RotateLeft(a + G(b, c, d) + x[1] + k, 3);
        d = RotateLeft(d + G(a, b, c) + x[5] + k, 5);
        c = RotateLeft(c + G(d, a, b) + x[9] + k, 9);
        b = RotateLeft(b + G(c, d, a) + x[13] + k, 13);
        a = RotateLeft(a + G(b, c, d) + x[2] + k, 3);
        d = RotateLeft(d + G(a, b, c) + x[6] + k, 5);
        c = RotateLeft(c + G(d, a, b) + x[10] + k, 9);
        b = RotateLeft(b + G(c, d, a) + x[14] + k, 13);
        a = RotateLeft(a + G(b, c, d) + x[3] + k, 3);
        d = RotateLeft(d + G(a, b, c) + x[7] + k, 5);
        c = RotateLeft(c + G(d, a, b) + x[11] + k, 9);
        b = RotateLeft(b + G(c, d, a) + x[15] + k, 13);
    }

    private static void RoundThree(
        ref uint a,
        ref uint b,
        ref uint c,
        ref uint d,
        uint[] x)
    {
        const uint k = 0x6ED9EBA1;
        a = RotateLeft(a + H(b, c, d) + x[0] + k, 3);
        d = RotateLeft(d + H(a, b, c) + x[8] + k, 9);
        c = RotateLeft(c + H(d, a, b) + x[4] + k, 11);
        b = RotateLeft(b + H(c, d, a) + x[12] + k, 15);
        a = RotateLeft(a + H(b, c, d) + x[2] + k, 3);
        d = RotateLeft(d + H(a, b, c) + x[10] + k, 9);
        c = RotateLeft(c + H(d, a, b) + x[6] + k, 11);
        b = RotateLeft(b + H(c, d, a) + x[14] + k, 15);
        a = RotateLeft(a + H(b, c, d) + x[1] + k, 3);
        d = RotateLeft(d + H(a, b, c) + x[9] + k, 9);
        c = RotateLeft(c + H(d, a, b) + x[5] + k, 11);
        b = RotateLeft(b + H(c, d, a) + x[13] + k, 15);
        a = RotateLeft(a + H(b, c, d) + x[3] + k, 3);
        d = RotateLeft(d + H(a, b, c) + x[11] + k, 9);
        c = RotateLeft(c + H(d, a, b) + x[7] + k, 11);
        b = RotateLeft(b + H(c, d, a) + x[15] + k, 15);
    }

    private static uint F(uint x, uint y, uint z) => (x & y) | (~x & z);

    private static uint G(uint x, uint y, uint z) => (x & y) | (x & z) | (y & z);

    private static uint H(uint x, uint y, uint z) => x ^ y ^ z;

    private static uint RotateLeft(uint value, int bits) => (value << bits) | (value >> (32 - bits));
}
