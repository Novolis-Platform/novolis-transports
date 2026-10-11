using System.Buffers.Binary;
using System.Net;
using System.Text;
using Novolis.Transports.Ed2k;

namespace Novolis.Transports.Kad;

/// <summary>
/// Encodes the bounded contact payloads used by the initial Kad operations.
/// </summary>
public static class KadWireCodec
{
    public static byte[] EncodeBootstrapRequest() => [];

    public static byte[] EncodeBootstrapResponse(
        KadContact local,
        IReadOnlyList<KadContact> contacts)
    {
        var writer = new KadWriter();
        WriteContactHeader(writer, local);
        writer.WriteUInt16(checked((ushort)contacts.Count));
        foreach (var contact in contacts)
            WriteContact(writer, contact);
        return writer.ToArray();
    }

    public static IReadOnlyList<KadContact> DecodeBootstrapResponse(ReadOnlySpan<byte> payload)
    {
        var reader = new KadReader(payload);
        _ = ReadContactHeader(ref reader);
        var count = reader.ReadUInt16();
        if (count > 10_000)
            throw new InvalidDataException("The Kad contact count is too large.");
        return ReadContacts(ref reader, count);
    }

    public static byte[] EncodeSearchRequest(KadId requester, Ed2kFileId fileId)
    {
        var writer = new KadWriter();
        writer.WriteBytes(fileId.Bytes);
        writer.WriteBytes(requester.Bytes);
        return writer.ToArray();
    }

    public static byte[] EncodeNodeRequest(KadId requester, KadId target)
    {
        var writer = new KadWriter();
        writer.WriteByte(0);
        writer.WriteBytes(target.Bytes);
        writer.WriteBytes(requester.Bytes);
        return writer.ToArray();
    }

    public static IReadOnlyList<KadContact> DecodeNodeResponse(ReadOnlySpan<byte> payload)
    {
        var reader = new KadReader(payload);
        _ = new KadId(reader.ReadBytes(16));
        var count = reader.ReadByte();
        return ReadContacts(ref reader, count);
    }

    public static byte[] EncodeNodeResponse(
        KadId target,
        IReadOnlyList<KadContact> contacts)
    {
        if (contacts.Count > byte.MaxValue)
            throw new ArgumentException("A Kad node response supports at most 255 contacts.");
        var writer = new KadWriter();
        writer.WriteBytes(target.Bytes);
        writer.WriteByte(checked((byte)contacts.Count));
        foreach (var contact in contacts)
            WriteContact(writer, contact);
        return writer.ToArray();
    }

    public static byte[] EncodePublishKeyword(
        KadId requester,
        Ed2kFileId fileId,
        string keyword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyword);
        var keywordBytes = Encoding.UTF8.GetBytes(keyword);
        if (keywordBytes.Length > byte.MaxValue)
            throw new ArgumentException("A Kad keyword is limited to 255 bytes.", nameof(keyword));

        var writer = new KadWriter();
        writer.WriteBytes(fileId.Bytes);
        writer.WriteBytes(requester.Bytes);
        writer.WriteByte(checked((byte)keywordBytes.Length));
        writer.WriteBytes(keywordBytes);
        return writer.ToArray();
    }

    public static byte[] EncodePublishSources(
        KadId requester,
        Ed2kFileId fileId,
        IReadOnlyList<KadSource> sources)
    {
        if (sources.Count > byte.MaxValue)
            throw new ArgumentException("A Kad publish message supports at most 255 sources.");

        var writer = new KadWriter();
        writer.WriteBytes(fileId.Bytes);
        writer.WriteBytes(requester.Bytes);
        writer.WriteByte(checked((byte)sources.Count));
        foreach (var source in sources)
        {
            writer.WriteBytes(source.NodeId.Bytes);
            writer.WriteBytes(source.Endpoint.Address.GetAddressBytes());
            writer.WriteUInt16(checked((ushort)source.Endpoint.Port));
            writer.WriteUInt16(checked((ushort)source.TcpPort));
        }

        return writer.ToArray();
    }

    public static IReadOnlyList<KadSource> DecodeSearchResponse(ReadOnlySpan<byte> payload)
    {
        var reader = new KadReader(payload);
        var count = reader.ReadByte();
        if (count > 255)
            throw new InvalidDataException("The Kad source count is too large.");
        var sources = new List<KadSource>(count);
        for (var index = 0; index < count; index++)
        {
            var id = new KadId(reader.ReadBytes(16));
            var address = new IPAddress(reader.ReadBytes(4));
            var udpPort = reader.ReadUInt16();
            var tcpPort = reader.ReadUInt16();
            sources.Add(new KadSource(id, new IPEndPoint(address, udpPort), tcpPort));
        }

        return sources;
    }

    private static void WriteContactHeader(KadWriter writer, KadContact contact)
    {
        writer.WriteBytes(contact.Id.Bytes);
        writer.WriteUInt16(checked((ushort)contact.TcpPort));
        writer.WriteByte(contact.Version);
    }

    private static void WriteContact(KadWriter writer, KadContact contact)
    {
        writer.WriteBytes(contact.Id.Bytes);
        writer.WriteBytes(contact.Endpoint.Address.GetAddressBytes());
        writer.WriteUInt16(checked((ushort)contact.Endpoint.Port));
        writer.WriteUInt16(checked((ushort)contact.TcpPort));
        writer.WriteByte(contact.Version);
    }

    private static KadContact ReadContactHeader(ref KadReader reader)
    {
        var id = new KadId(reader.ReadBytes(16));
        var tcpPort = reader.ReadUInt16();
        var version = reader.ReadByte();
        return new KadContact(id, new IPEndPoint(IPAddress.Any, 0), tcpPort, version);
    }

    private static IReadOnlyList<KadContact> ReadContacts(
        ref KadReader reader,
        int count)
    {
        var contacts = new List<KadContact>(count);
        for (var index = 0; index < count; index++)
        {
            var id = new KadId(reader.ReadBytes(16));
            var address = new IPAddress(reader.ReadBytes(4));
            var udpPort = reader.ReadUInt16();
            var tcpPort = reader.ReadUInt16();
            var version = reader.ReadByte();
            contacts.Add(new KadContact(id, new IPEndPoint(address, udpPort), tcpPort, version));
        }

        return contacts;
    }

    private sealed class KadWriter
    {
        private readonly MemoryStream stream = new();

        public void WriteByte(byte value) => this.stream.WriteByte(value);

        public void WriteBytes(ReadOnlySpan<byte> value) => this.stream.Write(value);

        public void WriteUInt16(ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
            this.WriteBytes(buffer);
        }

        public byte[] ToArray() => this.stream.ToArray();
    }

    private ref struct KadReader
    {
        private readonly ReadOnlySpan<byte> data;
        private int offset;

        public KadReader(ReadOnlySpan<byte> data)
        {
            this.data = data;
            this.offset = 0;
        }

        public byte ReadByte()
        {
            Require(1);
            return this.data[this.offset++];
        }

        public ushort ReadUInt16()
        {
            Require(2);
            var value = BinaryPrimitives.ReadUInt16LittleEndian(this.data[this.offset..]);
            this.offset += 2;
            return value;
        }

        public byte[] ReadBytes(int count)
        {
            Require(count);
            var value = this.data.Slice(this.offset, count).ToArray();
            this.offset += count;
            return value;
        }

        private void Require(int count)
        {
            if (count < 0 || count > this.data.Length - this.offset)
                throw new InvalidDataException("The Kad payload is truncated.");
        }
    }
}
