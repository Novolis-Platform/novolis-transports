using System.Buffers.Binary;
using System.Net;
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
