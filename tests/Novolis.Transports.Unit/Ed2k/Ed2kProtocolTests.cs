using System.Net;
using System.Text;
using Novolis.Transports.Ed2k;

namespace Novolis.Transports.Unit.Ed2k;

public sealed class Ed2kProtocolTests
{
    [Test]
    public async Task Server_list_roundtrips_text_and_server_met()
    {
        var text = ServerList.ParseText(
            """
            # user supplied servers
            127.0.0.1 4661
            example.invalid:4661
            """.Replace("example.invalid:4661", "example.invalid 4661", StringComparison.Ordinal));
        var binary = ServerList.EncodeServerMet(
        [
            new Ed2kServerInfo(
                new Ed2kServerEndpoint("127.0.0.1", 4661),
                "local",
                12,
                34)
        ]);
        var parsed = ServerList.ParseServerMet(binary);

        await Assert.That(text.Servers.Count).IsEqualTo(2);
        await Assert.That(parsed.Servers.Count).IsEqualTo(1);
        await Assert.That(parsed.Servers[0].Endpoint.Host).IsEqualTo("127.0.0.1");
        await Assert.That(parsed.Servers[0].Name).IsEqualTo("local");
        await Assert.That(parsed.Servers[0].UserCount).IsEqualTo((uint)12);
    }

    [Test]
    public async Task Compressed_payload_roundtrips()
    {
        var original = Encoding.UTF8.GetBytes("eD2k compressed payload");
        var compressed = Ed2kCompression.Compress(original);

        await Assert.That(Ed2kCompression.Decompress(compressed)).IsEquivalentTo(original);
    }

    [Test]
    public async Task Aich_verifies_blocks_and_rejects_changed_data()
    {
        var data = new byte[Ed2kProtocol.AichBlockSize + 17];
        data[0] = 1;
        data[^1] = 2;
        var tree = AichHashTree.Compute(data);

        await Assert.That(tree.BlockHashes.Count).IsEqualTo(2);
        await Assert.That(tree.VerifyBlock(0, data.AsSpan(0, Ed2kProtocol.AichBlockSize))).IsTrue();
        await Assert.That(tree.VerifyFile(data)).IsTrue();
        data[0] = 9;
        await Assert.That(tree.VerifyFile(data)).IsFalse();
    }

    [Test]
    public async Task Credits_apply_pairwise_modifier_and_queue_score()
    {
        var peer = Enumerable.Repeat((byte)0x42, 16).ToArray();
        var credits = new Ed2kCredits();
        credits.RecordUpload(peer, 4 * 1024 * 1024);
        credits.RecordDownload(peer, 2 * 1024 * 1024);

        var record = credits.GetRecord(peer);
        var modifier = credits.GetModifier(peer);
        var score = credits.GetQueueScore(peer, TimeSpan.FromSeconds(10));

        await Assert.That(record.UploadedBytes).IsEqualTo(4 * 1024 * 1024);
        await Assert.That(modifier).IsGreaterThan(1);
        await Assert.That(score).IsGreaterThan(10);
    }

    [Test]
    public async Task Secure_identity_signs_and_verifies_challenges()
    {
        using var identity = new Ed2kSecureIdentity();
        var challenge = identity.CreateChallenge();
        var signature = identity.Sign(challenge);

        await Assert.That(identity.Verify(challenge, signature)).IsTrue();
        challenge[0] ^= 0xFF;
        await Assert.That(identity.Verify(challenge, signature)).IsFalse();
    }

    [Test]
    public async Task Loopback_peer_transfers_and_verifies_a_part()
    {
        var data = Encoding.UTF8.GetBytes("public-domain loopback fixture");
        var fileId = Ed2kFileId.Compute(data);
        var userHash = Enumerable.Repeat((byte)0x51, 16).ToArray();
        var destination = Path.Combine(
            Path.GetTempPath(),
            $"novolis-ed2k-{Guid.NewGuid():N}.bin");

        await using var host = new Ed2kPeerHost();
        host.Share(fileId, data);
        await host.StartAsync();
        try
        {
            await Ed2kPeerSession.DownloadAsync(
                host.LocalEndpoint,
                fileId,
                data.Length,
                destination,
                userHash);
            await Assert.That(await File.ReadAllBytesAsync(destination)).IsEquivalentTo(data);
            await Assert.That(host.Credits.GetRecord(userHash).UploadedBytes).IsEqualTo(data.Length);
        }
        finally
        {
            File.Delete(destination);
        }
    }
}
