using System.Net;
using Novolis.Transports.Ed2k;
using Novolis.Transports.Kad;

namespace Novolis.Transports.Unit.Kad;

public sealed class KadProtocolTests
{
    [Test]
    public async Task Xor_distance_orders_closer_contacts_first()
    {
        var local = KadId.Parse("00000000000000000000000000000000");
        var close = KadId.Parse("00000000000000000000000000000001");
        var far = KadId.Parse("80000000000000000000000000000000");
        var table = new KadRoutingTable(local);
        table.TryAdd(new KadContact(close, new IPEndPoint(IPAddress.Loopback, 4672), 4662));
        table.TryAdd(new KadContact(far, new IPEndPoint(IPAddress.Loopback, 4673), 4662));

        var result = table.FindClosest(close, 2);

        await Assert.That(result[0].Id).IsEqualTo(close);
        await Assert.That(local.GetBucketIndex(close)).IsEqualTo(127);
    }

    [Test]
    public async Task Bootstrap_contacts_roundtrip()
    {
        var local = new KadContact(
            KadId.Parse("00000000000000000000000000000001"),
            new IPEndPoint(IPAddress.Loopback, 4672),
            4662,
            8);
        var contact = new KadContact(
            KadId.Parse("00000000000000000000000000000002"),
            new IPEndPoint(IPAddress.Parse("192.0.2.10"), 4673),
            4663,
            9);

        var decoded = KadWireCodec.DecodeBootstrapResponse(
            KadWireCodec.EncodeBootstrapResponse(local, [contact]));

        await Assert.That(decoded.Count).IsEqualTo(1);
        await Assert.That(decoded[0]).IsEqualTo(contact);
    }

    [Test]
    public async Task Packed_packet_roundtrips_through_zlib()
    {
        var packet = new KadPacket(KadProtocol.Header, KadProtocol.SearchResponse, [1, 2, 3]);

        var decoded = KadPacketCodec.Decode(packet.Encode(compressed: true));

        await Assert.That(decoded.Opcode).IsEqualTo(KadProtocol.SearchResponse);
        await Assert.That(decoded.Payload).IsEquivalentTo(packet.Payload);
    }

    [Test]
    public async Task Publish_sources_roundtrip()
    {
        var id = KadId.CreateRandom();
        var fileId = Ed2kFileId.Parse("00112233445566778899aabbccddeeff");
        var source = new KadSource(
            id,
            new IPEndPoint(IPAddress.Parse("192.0.2.20"), 4672),
            4662);
        var payload = KadWireCodec.EncodePublishSources(id, fileId, [source]);
        var decoded = KadWireCodec.DecodeSearchResponse(payload.AsSpan(32));

        await Assert.That(decoded[0].NodeId).IsEqualTo(id);
        await Assert.That(decoded[0].Endpoint).IsEqualTo(source.Endpoint);
    }
}
