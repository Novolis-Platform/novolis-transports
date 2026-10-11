using System.Text;
using Novolis.Transports.Ed2k;

namespace Novolis.Transports.Unit.Ed2k;

public sealed class Ed2kWireRoundTripTests
{
    [Test]
    public async Task Md4_matches_rfc_vectors()
    {
        var vectors = new Dictionary<string, string>
        {
            [""] = "31d6cfe0d16ae931b73c59d7e0c089c0",
            ["a"] = "bde52cb31de33e46245e05fbdbd6fb24",
            ["abc"] = "a448017aaf21d8525fc10ae87aa6729d",
            ["message digest"] = "d9130a8164549fe818874806e1c7014b",
            ["abcdefghijklmnopqrstuvwxyz"] = "d79e1c308aa5bbcdeea8ed63df412da9"
        };

        foreach (var pair in vectors)
        {
            var digest = Ed2kMd4.Compute(Encoding.ASCII.GetBytes(pair.Key));
            await Assert.That(Convert.ToHexString(digest).ToLowerInvariant()).IsEqualTo(pair.Value);
        }
    }

    [Test]
    public async Task Tcp_frame_uses_little_endian_length_including_opcode()
    {
        var frame = new Ed2kFrame(Ed2kProtocol.EdonkeyProtocol, Ed2kProtocol.SearchResults, [0xAA, 0xBB]);
        var encoded = frame.EncodeTcp();

        await Assert.That(encoded).IsEquivalentTo(
            new byte[] { 0xE3, 0x03, 0x00, 0x00, 0x00, 0x33, 0xAA, 0xBB });
        var decoded = Ed2kFrameCodec.DecodeTcp(encoded);
        await Assert.That(decoded.Protocol).IsEqualTo(frame.Protocol);
        await Assert.That(decoded.Opcode).IsEqualTo(frame.Opcode);
        await Assert.That(decoded.Payload).IsEquivalentTo(frame.Payload);
    }

    [Test]
    public async Task Udp_frame_roundtrips_without_length_field()
    {
        var frame = new Ed2kFrame(Ed2kProtocol.EmuleProtocol, 0x40, [1, 2, 3]);
        var decoded = Ed2kFrameCodec.TryDecodeUdp(frame.EncodeUdp(), out var roundTrip);

        await Assert.That(decoded).IsTrue();
        await Assert.That(roundTrip!.Protocol).IsEqualTo(frame.Protocol);
        await Assert.That(roundTrip.Opcode).IsEqualTo(frame.Opcode);
        await Assert.That(roundTrip.Payload).IsEquivalentTo(frame.Payload);
    }

    [Test]
    public async Task Tags_roundtrip_numeric_and_named_values()
    {
        var tags = new[]
        {
            new Ed2kTag(Ed2kTagIds.Name, "sample.bin"),
            new Ed2kTag(Ed2kTagIds.Size, (uint)42),
            new Ed2kTag("users", "12")
        };

        var decoded = Ed2kTagCodec.Decode(Ed2kTagCodec.Encode(tags));

        await Assert.That(decoded.Count).IsEqualTo(3);
        await Assert.That(Ed2kTagCodec.Find(decoded, Ed2kTagIds.Name)?.StringValue)
            .IsEqualTo("sample.bin");
        await Assert.That(Ed2kTagCodec.Find(decoded, Ed2kTagIds.Size)?.TryGetUInt32(out _))
            .IsTrue();
        await Assert.That(Ed2kTagCodec.Find(decoded, "users")?.StringValue).IsEqualTo("12");
    }

    [Test]
    public async Task File_id_hashes_part_digests_for_large_files()
    {
        var data = new byte[Ed2kProtocol.PartSize + 1];
        data[0] = 0x11;
        data[^1] = 0x22;

        var first = Ed2kMd4.Compute(data.AsSpan(0, Ed2kProtocol.PartSize));
        var second = Ed2kMd4.Compute(data.AsSpan(Ed2kProtocol.PartSize, 1));
        var expected = Ed2kMd4.Compute([.. first, .. second]);

        await Assert.That(Ed2kFileId.Compute(data).ToString())
            .IsEqualTo(Convert.ToHexString(expected).ToLowerInvariant());
    }

    [Test]
    public async Task Links_roundtrip_file_and_server_forms()
    {
        var fileId = Ed2kFileId.Parse("00112233445566778899aabbccddeeff");
        var file = Ed2kLink.CreateFile("a sample.bin", 42, fileId);
        var server = Ed2kLink.CreateServer(new Ed2kServerEndpoint("127.0.0.1", 4661));

        await Assert.That(Ed2kLink.Parse(file.ToString()).FileId).IsEqualTo(fileId);
        await Assert.That(Ed2kLink.Parse(file.ToString()).FileName).IsEqualTo("a sample.bin");
        await Assert.That(Ed2kLink.Parse(server.ToString()).Server).IsEqualTo(server.Server);
    }
}
