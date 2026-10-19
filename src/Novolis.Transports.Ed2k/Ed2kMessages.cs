using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Novolis.Transports.Ed2k;

/// <summary>
/// Creates and parses the common eD2k server and peer payloads.
/// </summary>
public static class Ed2kMessages
{
    public static Ed2kFrame CreateLogin(
        ReadOnlySpan<byte> userHash,
        int listenPort,
        string clientName = "Novolis",
        uint clientId = 0)
    {
        if (userHash.Length != 16)
            throw new ArgumentException("The eD2k user hash must contain 16 bytes.", nameof(userHash));
        if (listenPort is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(listenPort));

        var tags = new[]
        {
            new Ed2kTag(Ed2kTagIds.Name, clientName),
            new Ed2kTag(Ed2kTagIds.ClientVersion, 0x3Cu),
            new Ed2kTag(Ed2kTagIds.ServerFlags, 0x0000_011Fu),
            new Ed2kTag(Ed2kTagIds.EmuleVersion, 0x0100_0000u)
        };
        var writer = new Ed2kBinaryWriter();
        writer.WriteBytes(userHash);
        writer.WriteUInt32(clientId);
        writer.WriteUInt16(checked((ushort)listenPort));
        writer.WriteBytes(Ed2kTagCodec.Encode(tags));
        return new Ed2kFrame(Ed2kProtocol.EdonkeyProtocol, Ed2kProtocol.LoginRequest, writer.ToArray());
    }

    public static Ed2kFrame CreateSearchRequest(string query)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        var bytes = Encoding.UTF8.GetBytes(query);
        if (bytes.Length > ushort.MaxValue)
            throw new ArgumentException("The search query is too long.", nameof(query));

        var writer = new Ed2kBinaryWriter();
        writer.WriteByte(0x01);
        writer.WriteUInt16(checked((ushort)bytes.Length));
        writer.WriteBytes(bytes);
        return new Ed2kFrame(
            Ed2kProtocol.EdonkeyProtocol,
            Ed2kProtocol.SearchRequest,
            writer.ToArray());
    }

    public static Ed2kFrame CreateSourceRequest(Ed2kFileId fileId)
    {
        var writer = new Ed2kBinaryWriter();
        writer.WriteBytes(fileId.Bytes);
        return new Ed2kFrame(
            Ed2kProtocol.EdonkeyProtocol,
            Ed2kProtocol.GetSources,
            writer.ToArray());
    }

    public static Ed2kFrame CreateOfferFiles(IReadOnlyList<Ed2kSharedFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);
        var writer = new Ed2kBinaryWriter();
        writer.WriteUInt32(checked((uint)files.Count));
        foreach (var file in files)
        {
            writer.WriteBytes(file.FileId.Bytes);
            var tags = new List<Ed2kTag>
            {
                new(Ed2kTagIds.Name, file.Name),
                new(Ed2kTagIds.Size, checked((uint)file.Size))
            };
            if (file.AdditionalTags is not null)
                tags.AddRange(file.AdditionalTags);
            writer.WriteBytes(Ed2kTagCodec.Encode(tags));
        }

        return new Ed2kFrame(
            Ed2kProtocol.EdonkeyProtocol,
            Ed2kProtocol.OfferFiles,
            writer.ToArray());
    }

    public static IReadOnlyList<Ed2kSearchResult> ParseSearchResults(
        ReadOnlySpan<byte> payload)
    {
        var reader = new Ed2kBinaryReader(payload);
        var count = reader.ReadUInt32();
        if (count > 100_000)
            throw new InvalidDataException("The search result count is too large.");

        var results = new List<Ed2kSearchResult>(checked((int)count));
        for (var index = 0; index < count; index++)
        {
            var fileId = new Ed2kFileId(reader.ReadBytes(16));
            var clientId = reader.ReadUInt32();
            var port = reader.ReadUInt16();
            var tags = Ed2kTagCodec.ReadTags(ref reader);
            var name = Ed2kTagCodec.Find(tags, Ed2kTagIds.Name)?.StringValue ?? string.Empty;
            var size = ReadSize(tags);
            var type = Ed2kTagCodec.Find(tags, Ed2kTagIds.Type)?.StringValue;
            var sourceCount = ReadInt(tags, Ed2kTagIds.SourceCount);
            var completeSourceCount =
                ReadOptionalInt(tags, Ed2kTagIds.CompleteSourceCount)
                ?? ReadNamedInt(
                    tags,
                    "complete_sources",
                    "complete sources",
                    "complete");
            var speed = ReadNamedLong(
                tags,
                "speed",
                "speeddown",
                "transfer_speed",
                "source_speed");
            var availability = ReadNamedInt(
                tags,
                "availability",
                "availability_percent",
                "availabilitypercentage");
            results.Add(new Ed2kSearchResult(
                fileId,
                clientId,
                port,
                string.IsNullOrEmpty(name) ? fileId.ToString() : name,
                size,
                type,
                sourceCount,
                speed,
                completeSourceCount,
                availability));
        }

        return results;
    }

    public static IReadOnlyList<Ed2kSource> ParseSources(
        ReadOnlySpan<byte> payload)
    {
        var reader = new Ed2kBinaryReader(payload);
        var fileId = new Ed2kFileId(reader.ReadBytes(16));
        _ = fileId;
        var count = reader.ReadByte();
        var sources = new List<Ed2kSource>(count);
        for (var index = 0; index < count; index++)
        {
            var clientId = reader.ReadUInt32();
            var port = reader.ReadUInt16();
            var address = new IPAddress(
                new[]
                {
                    (byte)(clientId & 0xFF),
                    (byte)((clientId >> 8) & 0xFF),
                    (byte)((clientId >> 16) & 0xFF),
                    (byte)((clientId >> 24) & 0xFF)
                });
            sources.Add(new Ed2kSource(clientId, address.ToString(), port));
        }

        return sources;
    }

    public static bool TryReadIdChange(
        Ed2kFrame frame,
        out uint clientId,
        out int assignedPort)
    {
        clientId = 0;
        assignedPort = 0;
        if (frame.Opcode != Ed2kProtocol.IdChange || frame.Payload.Length < 4)
            return false;

        var reader = new Ed2kBinaryReader(frame.Payload);
        clientId = reader.ReadUInt32();
        assignedPort = reader.Remaining >= 2 ? reader.ReadUInt16() : 0;
        return true;
    }

    public static bool IsHighId(uint clientId) => clientId >= 16_777_216;

    public static byte[] CreateUserHash()
    {
        var hash = new byte[16];
        RandomNumberGenerator.Fill(hash);
        return hash;
    }

    private static long ReadSize(IReadOnlyList<Ed2kTag> tags)
    {
        var tag = Ed2kTagCodec.Find(tags, Ed2kTagIds.Size);
        if (tag?.Value is uint unsigned)
            return unsigned;
        if (tag?.Value is ulong large)
            return checked((long)large);
        if (tag?.Value is ushort small)
            return small;
        return 0;
    }

    private static int ReadInt(IReadOnlyList<Ed2kTag> tags, byte id)
    {
        var tag = Ed2kTagCodec.Find(tags, id);
        return ReadOptionalInt(tag) ?? 0;
    }

    private static int? ReadOptionalInt(
        IReadOnlyList<Ed2kTag> tags,
        byte id) =>
        ReadOptionalInt(Ed2kTagCodec.Find(tags, id));

    private static int? ReadNamedInt(
        IReadOnlyList<Ed2kTag> tags,
        params string[] names)
    {
        foreach (var name in names)
        {
            var value = ReadOptionalInt(Ed2kTagCodec.Find(tags, name));
            if (value is not null)
                return value;
        }

        return null;
    }

    private static long? ReadNamedLong(
        IReadOnlyList<Ed2kTag> tags,
        params string[] names)
    {
        foreach (var name in names)
        {
            var value = ReadOptionalLong(Ed2kTagCodec.Find(tags, name));
            if (value is not null)
                return value;
        }

        return null;
    }

    private static int? ReadOptionalInt(Ed2kTag? tag)
    {
        var value = ReadOptionalLong(tag);
        return value is null ? null : checked((int)value.Value);
    }

    private static long? ReadOptionalLong(Ed2kTag? tag)
    {
        if (tag?.Value is string text)
            return long.TryParse(text, out var parsed) ? parsed : null;

        return tag?.Value switch
        {
            byte value => value,
            ushort value => value,
            uint value => value,
            ulong value => checked((long)value),
            _ => null
        };
    }
}
