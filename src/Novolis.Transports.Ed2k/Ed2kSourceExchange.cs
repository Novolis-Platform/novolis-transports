namespace Novolis.Transports.Ed2k;

/// <summary>
/// Compact source-exchange payload for a single eD2k file.
/// </summary>
public static class Ed2kSourceExchange
{
    public static byte[] Encode(Ed2kFileId fileId, IReadOnlyList<Ed2kSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        if (sources.Count > byte.MaxValue)
            throw new ArgumentException("An eD2k source-exchange message supports at most 255 sources.");

        var writer = new Ed2kBinaryWriter();
        writer.WriteBytes(fileId.Bytes);
        writer.WriteByte(checked((byte)sources.Count));
        foreach (var source in sources)
        {
            if (!uint.TryParse(source.Address.Replace(".", string.Empty, StringComparison.Ordinal), out _))
            {
                if (!System.Net.IPAddress.TryParse(source.Address, out var parsed))
                    throw new ArgumentException("Source addresses must be IPv4.", nameof(sources));
                writer.WriteBytes(parsed.GetAddressBytes());
            }
            else
            {
                var address = System.Net.IPAddress.Parse(source.Address);
                writer.WriteBytes(address.GetAddressBytes());
            }

            writer.WriteUInt16(checked((ushort)source.Port));
            writer.WriteUInt16(checked((ushort)Math.Max(0, source.KadPort)));
        }

        return writer.ToArray();
    }

    public static (Ed2kFileId FileId, IReadOnlyList<Ed2kSource> Sources) Decode(
        ReadOnlySpan<byte> payload)
    {
        var reader = new Ed2kBinaryReader(payload);
        var fileId = new Ed2kFileId(reader.ReadBytes(16));
        var count = reader.ReadByte();
        var sources = new List<Ed2kSource>(count);
        for (var index = 0; index < count; index++)
        {
            var address = new System.Net.IPAddress(reader.ReadBytes(4)).ToString();
            var port = reader.ReadUInt16();
            var kadPort = reader.ReadUInt16();
            sources.Add(new Ed2kSource(0, address, port, kadPort));
        }

        return (fileId, sources);
    }
}
