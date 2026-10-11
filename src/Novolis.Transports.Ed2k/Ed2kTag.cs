namespace Novolis.Transports.Ed2k;

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
