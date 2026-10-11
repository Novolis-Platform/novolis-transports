using System.Text;

namespace Novolis.Transports.Ed2k;

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
