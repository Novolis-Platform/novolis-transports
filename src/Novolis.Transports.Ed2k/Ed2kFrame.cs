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
