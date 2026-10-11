using System.Text;

namespace Novolis.Transports.Ed2k;

/// <summary>
/// Wire value types used by eD2k and eMule tags.
/// </summary>
public enum Ed2kTagType : byte
{
    String = 0x02,
    UInt32 = 0x03,
    Binary = 0x04,
    UInt16 = 0x08,
    Byte = 0x09,
    UInt64 = 0x0B
}

/// <summary>
/// A named or numeric eD2k tag.
/// </summary>
public sealed class Ed2kTag
{
    public Ed2kTag(byte id, string value, bool useShortName = true)
        : this(Ed2kTagType.String, id, null, value, useShortName)
    {
    }

    public Ed2kTag(byte id, uint value, bool useShortName = true)
        : this(Ed2kTagType.UInt32, id, null, value, useShortName)
    {
    }

    public Ed2kTag(byte id, ushort value, bool useShortName = true)
        : this(Ed2kTagType.UInt16, id, null, value, useShortName)
    {
    }

    public Ed2kTag(byte id, byte value, bool useShortName = true)
        : this(Ed2kTagType.Byte, id, null, value, useShortName)
    {
    }

    public Ed2kTag(byte id, ulong value, bool useShortName = true)
        : this(Ed2kTagType.UInt64, id, null, value, useShortName)
    {
    }

    public Ed2kTag(byte id, byte[] value, bool useShortName = true)
        : this(Ed2kTagType.Binary, id, null, value.ToArray(), useShortName)
    {
    }

    public Ed2kTag(string name, string value)
        : this(Ed2kTagType.String, null, name, value, false)
    {
    }

    private Ed2kTag(
        Ed2kTagType type,
        byte? id,
        string? name,
        object value,
        bool useShortName)
    {
        if (id is null && string.IsNullOrEmpty(name))
            throw new ArgumentException("A tag needs either an id or a name.");

        this.Type = type;
        this.Id = id;
        this.Name = name;
        this.Value = value;
        this.UseShortName = useShortName && id is not null;
    }

    public byte? Id { get; }

    public string? Name { get; }

    public Ed2kTagType Type { get; }

    public object Value { get; }

    public bool UseShortName { get; }

    public string? StringValue => this.Value as string;

    public byte[]? BinaryValue => this.Value as byte[];

    public bool TryGetUInt32(out uint value)
    {
        if (this.Value is uint typed)
        {
            value = typed;
            return true;
        }

        value = 0;
        return false;
    }

    public override string ToString()
    {
        var name = this.Id is { } id ? $"0x{id:X2}" : this.Name;
        return $"{name}={this.Value}";
    }

    internal static Ed2kTag FromDecoded(
        Ed2kTagType type,
        byte? id,
        string? name,
        object value,
        bool useShortName)
    {
        return new Ed2kTag(type, id, name, value, useShortName);
    }
}

/// <summary>
/// Well-known numeric tag ids used by the server and client messages.
/// </summary>
public static class Ed2kTagIds
{
    public const byte Name = 0x01;
    public const byte Size = 0x02;
    public const byte Type = 0x03;
    public const byte Format = 0x04;
    public const byte SourceCount = 0x15;
    public const byte ClientVersion = 0x11;
    public const byte ServerFlags = 0x20;
    public const byte EmuleVersion = 0xFB;
}

/// <summary>
/// Encodes and decodes the tag sequences used inside eD2k messages.
/// </summary>
public static class Ed2kTagCodec
{
    public static byte[] Encode(IReadOnlyList<Ed2kTag> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);
        var writer = new Ed2kBinaryWriter();
        writer.WriteUInt32(checked((uint)tags.Count));
        foreach (var tag in tags)
            WriteTag(writer, tag);
        return writer.ToArray();
    }

    public static IReadOnlyList<Ed2kTag> Decode(ReadOnlySpan<byte> data)
    {
        var reader = new Ed2kBinaryReader(data);
        return ReadTags(ref reader);
    }

    internal static IReadOnlyList<Ed2kTag> ReadTags(ref Ed2kBinaryReader reader)
    {
        var count = reader.ReadUInt32();
        if (count > 100_000)
            throw new InvalidDataException("The eD2k tag count is too large.");

        var tags = new List<Ed2kTag>(checked((int)count));
        for (var index = 0; index < count; index++)
            tags.Add(ReadTag(ref reader));
        return tags;
    }

    public static Ed2kTag? Find(
        IReadOnlyList<Ed2kTag> tags,
        byte id)
    {
        return tags.FirstOrDefault(tag => tag.Id == id);
    }

    public static Ed2kTag? Find(
        IReadOnlyList<Ed2kTag> tags,
        string name)
    {
        return tags.FirstOrDefault(
            tag => string.Equals(tag.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static void WriteTag(Ed2kBinaryWriter writer, Ed2kTag tag)
    {
        var type = (byte)tag.Type;
        if (tag.UseShortName)
        {
            writer.WriteByte((byte)(type | 0x80));
            writer.WriteByte(tag.Id!.Value);
        }
        else
        {
            writer.WriteByte(type);
            if (tag.Id is { } id)
            {
                writer.WriteUInt16(1);
                writer.WriteByte(id);
            }
            else
            {
                var nameBytes = Encoding.UTF8.GetBytes(tag.Name!);
                writer.WriteUInt16(checked((ushort)nameBytes.Length));
                writer.WriteBytes(nameBytes);
            }
        }

        switch (tag.Type)
        {
            case Ed2kTagType.String:
                var stringBytes = Encoding.UTF8.GetBytes((string)tag.Value);
                writer.WriteUInt16(checked((ushort)stringBytes.Length));
                writer.WriteBytes(stringBytes);
                break;
            case Ed2kTagType.UInt32:
                writer.WriteUInt32((uint)tag.Value);
                break;
            case Ed2kTagType.Binary:
                var bytes = (byte[])tag.Value;
                writer.WriteUInt32(checked((uint)bytes.Length));
                writer.WriteBytes(bytes);
                break;
            case Ed2kTagType.UInt16:
                writer.WriteUInt16((ushort)tag.Value);
                break;
            case Ed2kTagType.Byte:
                writer.WriteByte((byte)tag.Value);
                break;
            case Ed2kTagType.UInt64:
                writer.WriteUInt64((ulong)tag.Value);
                break;
            default:
                throw new InvalidDataException($"Unsupported eD2k tag type {tag.Type}.");
        }
    }

    private static Ed2kTag ReadTag(ref Ed2kBinaryReader reader)
    {
        var rawType = reader.ReadByte();
        var useShortName = (rawType & 0x80) != 0;
        var type = (Ed2kTagType)(rawType & 0x7F);
        byte? id = null;
        string? name = null;

        if (useShortName)
        {
            id = reader.ReadByte();
        }
        else
        {
            var nameLength = reader.ReadUInt16();
            if (nameLength == 1)
                id = reader.ReadByte();
            else
                name = reader.ReadString(nameLength);
        }

        object value;
        switch (type)
        {
            case Ed2kTagType.String:
                value = reader.ReadString(reader.ReadUInt16());
                break;
            case Ed2kTagType.UInt32:
                value = reader.ReadUInt32();
                break;
            case Ed2kTagType.Binary:
                value = reader.ReadBytes(checked((int)reader.ReadUInt32()));
                break;
            case Ed2kTagType.UInt16:
                value = reader.ReadUInt16();
                break;
            case Ed2kTagType.Byte:
                value = reader.ReadByte();
                break;
            case Ed2kTagType.UInt64:
                value = reader.ReadUInt64();
                break;
            default:
                if ((byte)type is >= 0x11 and <= 0x20)
                    value = reader.ReadString((byte)type - 0x10);
                else
                    throw new InvalidDataException($"Unsupported eD2k tag type {type}.");
                break;
        }

        return Ed2kTag.FromDecoded(type, id, name, value, useShortName);
    }
}
