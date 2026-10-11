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
