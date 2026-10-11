using System.Buffers.Binary;
using System.Net;
using System.Text;

namespace Novolis.Transports.Ed2k;

internal ref struct Ed2kBinaryReader
{
    private readonly ReadOnlySpan<byte> data;
    private int offset;

    public Ed2kBinaryReader(ReadOnlySpan<byte> data)
    {
        this.data = data;
        this.offset = 0;
    }

    public int Remaining => this.data.Length - this.offset;

    public int Position => this.offset;

    public byte ReadByte()
    {
        this.Require(1);
        return this.data[this.offset++];
    }

    public ushort ReadUInt16()
    {
        this.Require(2);
        var value = BinaryPrimitives.ReadUInt16LittleEndian(this.data[this.offset..]);
        this.offset += 2;
        return value;
    }

    public uint ReadUInt32()
    {
        this.Require(4);
        var value = BinaryPrimitives.ReadUInt32LittleEndian(this.data[this.offset..]);
        this.offset += 4;
        return value;
    }

    public ulong ReadUInt64()
    {
        this.Require(8);
        var value = BinaryPrimitives.ReadUInt64LittleEndian(this.data[this.offset..]);
        this.offset += 8;
        return value;
    }

    public int ReadInt32() => unchecked((int)this.ReadUInt32());

    public byte[] ReadBytes(int count)
    {
        if (count < 0)
            throw new InvalidDataException("A byte count cannot be negative.");

        this.Require(count);
        var value = this.data.Slice(this.offset, count).ToArray();
        this.offset += count;
        return value;
    }

    public string ReadString(int byteCount)
    {
        return Encoding.UTF8.GetString(this.ReadBytes(byteCount));
    }

    public IPAddress ReadIPv4()
    {
        var bytes = this.ReadBytes(4);
        return new IPAddress(bytes);
    }

    private void Require(int count)
    {
        if (count > this.Remaining)
            throw new InvalidDataException("The eD2k payload is truncated.");
    }
}
