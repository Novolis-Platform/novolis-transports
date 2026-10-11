using System.Buffers.Binary;

namespace Novolis.Transports.Ed2k;

/// <summary>
/// One decoded eD2k or eMule message.
/// </summary>
public sealed record Ed2kFrame(byte Protocol, byte Opcode, byte[] Payload)
{
    public byte[] EncodeTcp()
    {
        var length = checked(Payload.Length + 1);
        var result = new byte[checked(length + 5)];
        result[0] = Protocol;
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(1, 4), (uint)length);
        result[5] = Opcode;
        Payload.CopyTo(result, 6);
        return result;
    }

    public byte[] EncodeUdp() 
    {
        var result = new byte[checked(Payload.Length + 2)];
        result[0] = Protocol;
        result[1] = Opcode;
        Payload.CopyTo(result, 2);
        return result;
    }
}

/// <summary>
/// Encodes and decodes the two eD2k frame forms.
/// </summary>
public static class Ed2kFrameCodec
{
    public static bool TryDecodeTcp(
        ReadOnlySpan<byte> data,
        out Ed2kFrame? frame,
        out int consumed)
    {
        frame = null;
        consumed = 0;
        if (data.Length < 5)
            return false;

        var length = BinaryPrimitives.ReadUInt32LittleEndian(data[1..5]);
        if (length == 0 || length > Ed2kProtocol.MaximumFramePayload + 1)
            throw new InvalidDataException($"Invalid eD2k frame length {length}.");

        var totalLength = checked((int)length + 5);
        if (data.Length < totalLength)
            return false;

        frame = new Ed2kFrame(
            data[0],
            data[5],
            data.Slice(6, checked((int)length - 1)).ToArray());
        consumed = totalLength;
        return true;
    }

    public static Ed2kFrame DecodeTcp(ReadOnlySpan<byte> data)
    {
        if (!TryDecodeTcp(data, out var frame, out var consumed) || consumed != data.Length)
            throw new InvalidDataException("The eD2k TCP buffer does not contain exactly one frame.");

        return frame!;
    }

    public static bool TryDecodeUdp(ReadOnlySpan<byte> data, out Ed2kFrame? frame)
    {
        frame = null;
        if (data.Length < 2)
            return false;

        if (data.Length - 2 > Ed2kProtocol.MaximumFramePayload)
            throw new InvalidDataException("The eD2k UDP payload is too large.");

        frame = new Ed2kFrame(data[0], data[1], data[2..].ToArray());
        return true;
    }

    public static async ValueTask<Ed2kFrame?> ReadTcpAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var header = await Ed2kBinary.ReadExactAsync(stream, 5, cancellationToken)
            .ConfigureAwait(false);
        if (header is null)
            return null;

        var length = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(1, 4));
        if (length == 0 || length > Ed2kProtocol.MaximumFramePayload + 1)
            throw new InvalidDataException($"Invalid eD2k frame length {length}.");

        var body = await Ed2kBinary.ReadExactAsync(
                stream,
                checked((int)length),
                cancellationToken)
            .ConfigureAwait(false);

        if (body is null || body.Length == 0)
            throw new EndOfStreamException("The eD2k stream ended before the opcode.");

        return new Ed2kFrame(header[0], body[0], body.AsSpan(1).ToArray());
    }
}
