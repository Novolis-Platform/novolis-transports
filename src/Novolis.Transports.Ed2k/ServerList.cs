using System.Buffers.Binary;
using System.Net;
using System.Text;

namespace Novolis.Transports.Ed2k;

/// <summary>
/// Metadata for a server-list entry.
/// </summary>
public sealed record Ed2kServerInfo(
    Ed2kServerEndpoint Endpoint,
    string? Name = null,
    uint? UserCount = null,
    uint? FileCount = null);

/// <summary>
/// Loads text server lists and the eMule server.met binary format.
/// </summary>
public sealed class ServerList
{
    public ServerList(IEnumerable<Ed2kServerInfo> servers)
    {
        ArgumentNullException.ThrowIfNull(servers);
        this.Servers = servers
            .Where(server => server.Endpoint.Port is >= 1 and <= 65535)
            .GroupBy(server => server.Endpoint.ToString(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
    }

    public IReadOnlyList<Ed2kServerInfo> Servers { get; }

    public static ServerList Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var bytes = File.ReadAllBytes(path);
        if (Path.GetExtension(path).Equals(".met", StringComparison.OrdinalIgnoreCase) ||
            (bytes.Length > 0 && (bytes[0] is 0xE0 or 0x0E)))
            return ParseServerMet(bytes);

        return ParseText(Encoding.UTF8.GetString(bytes));
    }

    public static ServerList ParseText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var servers = new List<Ed2kServerInfo>();
        foreach (var rawLine in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#') || line.StartsWith(';'))
                continue;

            var fields = line.Split(
                [' ', '\t', ','],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (fields.Length < 2 || !int.TryParse(fields[1], out var port))
                continue;

            servers.Add(new Ed2kServerInfo(new Ed2kServerEndpoint(fields[0], port)));
        }

        return new ServerList(servers);
    }

    public static ServerList ParseServerMet(ReadOnlySpan<byte> data)
    {
        var reader = new Ed2kBinaryReader(data);
        var version = reader.ReadByte();
        if (version is not (0xE0 or 0x0E))
            throw new InvalidDataException($"Unsupported server.met header 0x{version:X2}.");

        var count = reader.ReadUInt32();
        if (count > 100_000)
            throw new InvalidDataException("The server.met entry count is too large.");

        var servers = new List<Ed2kServerInfo>(checked((int)count));
        for (var index = 0; index < count; index++)
        {
            var ipValue = reader.ReadUInt32();
            var addressBytes = new byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(addressBytes, ipValue);
            var endpoint = new Ed2kServerEndpoint(
                new IPAddress(addressBytes).ToString(),
                reader.ReadUInt16());
            var tags = Ed2kTagCodec.ReadTags(ref reader);
            var name = Ed2kTagCodec.Find(tags, Ed2kTagIds.Name)?.StringValue
                       ?? Ed2kTagCodec.Find(tags, "name")?.StringValue;
            var users = ReadUInt32Tag(tags, "users");
            var files = ReadUInt32Tag(tags, "files");
            servers.Add(new Ed2kServerInfo(endpoint, name, users, files));
        }

        return new ServerList(servers);
    }

    public static byte[] EncodeServerMet(IReadOnlyList<Ed2kServerInfo> servers)
    {
        ArgumentNullException.ThrowIfNull(servers);
        var writer = new Ed2kBinaryWriter();
        writer.WriteByte(0xE0);
        writer.WriteUInt32(checked((uint)servers.Count));
        foreach (var server in servers)
        {
            if (!IPAddress.TryParse(server.Endpoint.Host, out var address) ||
                address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                throw new ArgumentException("server.met encoding requires IPv4 server hosts.");

            writer.WriteBytes(address.GetAddressBytes());
            writer.WriteUInt16(checked((ushort)server.Endpoint.Port));
            var tags = new List<Ed2kTag>();
            if (!string.IsNullOrEmpty(server.Name))
                tags.Add(new Ed2kTag(Ed2kTagIds.Name, server.Name));
            if (server.UserCount is { } users)
                tags.Add(new Ed2kTag("users", users.ToString()));
            if (server.FileCount is { } files)
                tags.Add(new Ed2kTag("files", files.ToString()));
            writer.WriteBytes(Ed2kTagCodec.Encode(tags));
        }

        return writer.ToArray();
    }

    private static uint? ReadUInt32Tag(IReadOnlyList<Ed2kTag> tags, string name)
    {
        var tag = Ed2kTagCodec.Find(tags, name);
        if (tag?.StringValue is { } text && uint.TryParse(text, out var value))
            return value;
        return tag?.TryGetUInt32(out var numeric) == true ? numeric : null;
    }
}
