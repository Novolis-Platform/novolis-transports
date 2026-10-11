using System.Buffers.Binary;
using System.Text;

namespace Novolis.Transports.Ed2k;

internal sealed class Ed2kBinaryWriter
{
    private readonly MemoryStream stream = new();

    public int Length => checked((int)this.stream.Length);

    public void WriteByte(byte value) => this.stream.WriteByte(value);

    public void WriteBytes(ReadOnlySpan<byte> value) => this.stream.Write(value);

    public void WriteUInt16(ushort value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        this.WriteBytes(buffer);
    }

    public void WriteUInt32(uint value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
        this.WriteBytes(buffer);
    }

    public void WriteUInt64(ulong value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
        this.WriteBytes(buffer);
    }

    public void WriteInt32(int value) => this.WriteUInt32(unchecked((uint)value));

    public void WriteString(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        this.WriteBytes(bytes);
    }

    public byte[] ToArray() => this.stream.ToArray();
}
