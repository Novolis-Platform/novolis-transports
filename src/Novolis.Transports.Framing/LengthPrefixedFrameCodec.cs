using System.Buffers.Binary;

namespace Novolis.Transports.Framing;

/// <summary>Reads and writes bounded little-endian length-prefixed frames.</summary>
public static class LengthPrefixedFrameCodec
{
    /// <summary>Default maximum payload accepted by the codec.</summary>
    public const int DefaultMaximumPayload = 64 * 1024 * 1024;

    /// <summary>Writes one frame and flushes the stream.</summary>
    public static async ValueTask WriteAsync(
        Stream stream,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (payload.Length > DefaultMaximumPayload)
            throw new ArgumentOutOfRangeException(nameof(payload));

        var prefix = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(prefix, payload.Length);
        await stream.WriteAsync(prefix, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reads one frame, or returns <see langword="null"/> at clean EOF.</summary>
    public static async ValueTask<LengthPrefixedFrame?> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var prefix = new byte[sizeof(int)];
        if (!await ReadExactlyAsync(stream, prefix, cancellationToken).ConfigureAwait(false))
            return null;

        var length = BinaryPrimitives.ReadInt32LittleEndian(prefix);
        if (length < 0 || length > DefaultMaximumPayload)
            throw new InvalidDataException($"Invalid frame length: {length}.");

        var payload = new byte[length];
        if (!await ReadExactlyAsync(stream, payload, cancellationToken).ConfigureAwait(false))
            throw new EndOfStreamException("The frame ended before its payload was complete.");

        return new LengthPrefixedFrame(payload);
    }

    private static async ValueTask<bool> ReadExactlyAsync(
        Stream stream,
        Memory<byte> buffer,
        CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(
                buffer[offset..],
                cancellationToken).ConfigureAwait(false);
            if (read == 0)
                return false;
            offset += read;
        }

        return true;
    }
}
