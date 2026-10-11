using Novolis.Transports.Ed2k;

namespace Novolis.Transports.Kad;

/// <summary>
/// A decoded Kad UDP packet.
/// </summary>
public sealed record KadPacket(byte Protocol, byte Opcode, byte[] Payload)
{
    public byte[] Encode(bool compressed = false)
    {
        var body = compressed ? Ed2kCompression.Compress(this.Payload) : this.Payload;
        var protocol = compressed ? KadProtocol.Packed : KadProtocol.Header;
        var result = new byte[body.Length + 2];
        result[0] = protocol;
        result[1] = this.Opcode;
        body.CopyTo(result, 2);
        return result;
    }
}

/// <summary>
/// Kad protocol marker and opcode constants.
/// </summary>
public static class KadProtocol
{
    public const byte Header = 0xE4;
    public const byte Packed = 0xE5;

    public const byte BootstrapRequest = 0x01;
    public const byte BootstrapResponse = 0x09;
    public const byte HelloRequest = 0x11;
    public const byte HelloResponse = 0x19;
    public const byte NodeRequest = 0x21;
    public const byte NodeResponse = 0x29;
    public const byte SearchKeyRequest = 0x33;
    public const byte SearchSourceRequest = 0x34;
    public const byte SearchNotesRequest = 0x35;
    public const byte SearchResponse = 0x3B;
    public const byte PublishKeyRequest = 0x43;
    public const byte PublishSourceRequest = 0x44;
    public const byte PublishNotesRequest = 0x45;
    public const byte PublishKeyResponse = 0x4B;
    public const byte PublishSourceResponse = 0x4C;
    public const byte Ping = 0x60;
    public const byte Pong = 0x61;
}

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
