using System.Buffers.Binary;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;

namespace Novolis.Transports.LocalIpc;

/// <summary>Encodes and decodes IPC frames using a simple length-prefixed binary envelope.</summary>
public static class LocalIpcFrameCodec
{
    private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

    public static async ValueTask WriteAsync(Stream stream, LocalIpcFrame frame, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(frame);

        await using var buffer = new MemoryStream();
        using (var writer = new BinaryWriter(buffer, Utf8, leaveOpen: true))
        {
            writer.Write(frame.Sequence);
            writer.Write(frame.Kind ?? string.Empty);
            writer.Write(frame.Name ?? string.Empty);
            writer.Write(frame.Payload.Length);
            writer.Write(frame.Payload);
            writer.Flush();
        }

        var payload = buffer.ToArray();
        Span<byte> lengthPrefix = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(lengthPrefix, payload.Length);
        stream.Write(lengthPrefix);
        await stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<LocalIpcFrame?> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var lengthPrefix = new byte[4];
        if (!await TryReadExactAsync(stream, lengthPrefix, cancellationToken).ConfigureAwait(false))
            return null;

        var length = BinaryPrimitives.ReadInt32LittleEndian(lengthPrefix);
        if (length < 0)
            throw new InvalidDataException("Negative frame length.");

        var payload = new byte[length];
        if (!await TryReadExactAsync(stream, payload, cancellationToken).ConfigureAwait(false))
            return null;

        using var buffer = new MemoryStream(payload, writable: false);
        using var reader = new BinaryReader(buffer, Utf8, leaveOpen: true);
        var sequence = reader.ReadInt64();
        var kind = reader.ReadString();
        var name = reader.ReadString();
        var payloadLength = reader.ReadInt32();
        if (payloadLength < 0)
            throw new InvalidDataException("Negative frame payload length.");

        var body = reader.ReadBytes(payloadLength);
        if (body.Length != payloadLength)
            throw new EndOfStreamException("Unexpected end of frame payload.");

        return new LocalIpcFrame(sequence, kind, name, body);
    }

    private static async ValueTask<bool> TryReadExactAsync(Stream stream, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.Slice(offset), cancellationToken).ConfigureAwait(false);
            if (read == 0)
                return false;

            offset += read;
        }

        return true;
    }
}
