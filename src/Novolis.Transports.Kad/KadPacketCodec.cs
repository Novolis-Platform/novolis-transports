using Novolis.Transports.Ed2k;

namespace Novolis.Transports.Kad;

/// <summary>
/// Kad UDP packet codec.
/// </summary>
public static class KadPacketCodec
{
    public static KadPacket Decode(ReadOnlySpan<byte> data)
    {
        if (data.Length < 2)
            throw new InvalidDataException("A Kad packet needs a protocol and opcode.");
        if (data[0] is not (KadProtocol.Header or KadProtocol.Packed))
            throw new InvalidDataException($"Unknown Kad protocol marker 0x{data[0]:X2}.");

        var payload = data[0] == KadProtocol.Packed
            ? Ed2kCompression.Decompress(data[2..])
            : data[2..].ToArray();
        return new KadPacket(data[0], data[1], payload);
    }
}
