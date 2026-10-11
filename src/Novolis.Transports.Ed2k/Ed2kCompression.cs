using System.IO.Compression;

namespace Novolis.Transports.Ed2k;

/// <summary>
/// zlib helpers for eMule compressed payloads.
/// </summary>
public static class Ed2kCompression
{
    public static byte[] Compress(ReadOnlySpan<byte> data)
    {
        using var output = new MemoryStream();
        using (var zlib = new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(data);
        return output.ToArray();
    }

    public static byte[] Decompress(ReadOnlySpan<byte> data, int maximumLength = Ed2kProtocol.MaximumFramePayload)
    {
        if (maximumLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumLength));

        using var input = new MemoryStream(data.ToArray(), writable: false);
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        var buffer = new byte[81920];
        while (true)
        {
            var read = zlib.Read(buffer, 0, buffer.Length);
            if (read == 0)
                break;
            if (output.Length + read > maximumLength)
                throw new InvalidDataException("The decompressed eD2k payload is too large.");
            output.Write(buffer, 0, read);
        }

        return output.ToArray();
    }
}
