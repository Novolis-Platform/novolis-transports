namespace Novolis.Transports.Ed2k;

internal static class Ed2kBinary
{
    public static byte[] ReadExact(Stream stream, int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var buffer = new byte[count];
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = stream.Read(buffer, offset, buffer.Length - offset);
            if (read == 0)
                throw new EndOfStreamException("The eD2k stream ended inside a frame.");
            offset += read;
        }

        return buffer;
    }

    public static async ValueTask<byte[]?> ReadExactAsync(
        Stream stream,
        int count,
        CancellationToken cancellationToken)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var buffer = new byte[count];
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(
                buffer.AsMemory(offset, buffer.Length - offset),
                cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                if (offset == 0)
                    return null;
                throw new EndOfStreamException("The eD2k stream ended inside a frame.");
            }

            offset += read;
        }

        return buffer;
    }
}
