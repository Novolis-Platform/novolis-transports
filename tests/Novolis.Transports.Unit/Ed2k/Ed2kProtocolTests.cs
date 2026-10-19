using System.Net;
using System.Buffers.Binary;
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
    public async Task Source_exchange_roundtrips_file_and_endpoints()
    {
        var fileId = Ed2kFileId.Parse("00112233445566778899aabbccddeeff");
        var sources = new[]
        {
            new Ed2kSource(1, "192.0.2.1", 4662, 4672)
        };

        var decoded = Ed2kSourceExchange.Decode(
            Ed2kSourceExchange.Encode(fileId, sources));

        await Assert.That(decoded.FileId).IsEqualTo(fileId);
        await Assert.That(decoded.Sources[0].Address).IsEqualTo("192.0.2.1");
        await Assert.That(decoded.Sources[0].KadPort).IsEqualTo(4672);
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
    public async Task Upload_queue_returns_the_highest_scored_peer()
    {
        var credits = new Ed2kCredits();
        var queue = new Ed2kUploadQueue(credits);
        var peer = Enumerable.Repeat((byte)0x44, 16).ToArray();
        queue.Enqueue(peer, queuedAt: DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1));

        var hasEntry = queue.TryTakeNext(DateTimeOffset.UtcNow, out var entry);

        await Assert.That(hasEntry).IsTrue();
        await Assert.That(entry?.PeerIdentity).IsEquivalentTo(peer);
        await Assert.That(queue.Count).IsEqualTo(0);
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

    [Test]
    public async Task Loopback_peer_transfers_multipart_file_and_verifies_file_id()
    {
        var data = new byte[Ed2kProtocol.PartSize + 1];
        data[0] = 0x17;
        data[Ed2kProtocol.PartSize - 1] = 0x28;
        data[^1] = 0x39;
        var fileId = Ed2kFileId.Compute(data);
        var userHash = Enumerable.Repeat((byte)0x61, 16).ToArray();
        var destination = Path.Combine(
            Path.GetTempPath(),
            $"novolis-ed2k-multipart-{Guid.NewGuid():N}.bin");

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
            var downloaded = await File.ReadAllBytesAsync(destination);
            await Assert.That(downloaded.SequenceEqual(data)).IsTrue();
            await Assert.That(host.Credits.GetRecord(userHash).UploadedBytes)
                .IsEqualTo(data.Length);
        }
        finally
        {
            File.Delete(destination);
        }
    }

    [Test]
    public async Task Session_logs_in_offers_files_and_searches()
    {
        var fileId = Ed2kFileId.Parse("00112233445566778899aabbccddeeff");
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var serverTask = Task.Run(async () =>
        {
            using var server = await listener.AcceptTcpClientAsync();
            await using var stream = server.GetStream();
            var login = await Ed2kFrameCodec.ReadTcpAsync(stream);
            if (login is null || login.Opcode != Ed2kProtocol.LoginRequest)
                throw new InvalidDataException("The fake server did not receive login.");

            var idPayload = new byte[6];
            BinaryPrimitives.WriteUInt32LittleEndian(idPayload.AsSpan(0, 4), 0x0100007F);
            BinaryPrimitives.WriteUInt16LittleEndian(idPayload.AsSpan(4, 2), 4662);
            await stream.WriteAsync(
                new Ed2kFrame(Ed2kProtocol.EdonkeyProtocol, Ed2kProtocol.IdChange, idPayload)
                    .EncodeTcp());
            await stream.FlushAsync();

            var offer = await Ed2kFrameCodec.ReadTcpAsync(stream);
            if (offer is null || offer.Opcode != Ed2kProtocol.OfferFiles)
                throw new InvalidDataException("The fake server did not receive offered files.");
            var search = await Ed2kFrameCodec.ReadTcpAsync(stream);
            if (search is null || search.Opcode != Ed2kProtocol.SearchRequest)
                throw new InvalidDataException("The fake server did not receive a search.");

            using var payload = new MemoryStream();
            var count = new byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(count, 1);
            payload.Write(count);
            payload.Write(fileId.ToArray());
            var clientId = new byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(clientId, 0x0200007F);
            payload.Write(clientId);
            var port = new byte[2];
            BinaryPrimitives.WriteUInt16LittleEndian(port, 4662);
            payload.Write(port);
            payload.Write(Ed2kTagCodec.Encode(
            [
                new Ed2kTag(Ed2kTagIds.Name, "fixture.bin"),
                new Ed2kTag(Ed2kTagIds.Size, (uint)123),
                new Ed2kTag(Ed2kTagIds.SourceCount, (uint)4),
                new Ed2kTag(Ed2kTagIds.CompleteSourceCount, (uint)2),
                new Ed2kTag("speed", (2 * 1024 * 1024).ToString()),
                new Ed2kTag("availability_percent", "75")
            ]));
            await stream.WriteAsync(
                new Ed2kFrame(
                        Ed2kProtocol.ZlibProtocol,
                        Ed2kProtocol.SearchResults,
                        Ed2kCompression.Compress(payload.ToArray()))
                    .EncodeTcp());
            await stream.FlushAsync();
        });

        try
        {
            await using var session = new Ed2kSession(
                clientName: "Test client",
                userHash: Enumerable.Repeat((byte)0x01, 16).ToArray());
            var connection = await session.ConnectServerAsync(
                new Ed2kServerEndpoint("127.0.0.1", endpoint.Port));
            await session.OfferFilesAsync(
            [
                new Ed2kSharedFile(fileId, "fixture.bin", 123)
            ]);
            var results = await session.SearchAsync("fixture");

            await Assert.That(connection.IsHighId).IsTrue();
            await Assert.That(results.Count).IsEqualTo(1);
            await Assert.That(results[0].Name).IsEqualTo("fixture.bin");
            await Assert.That(results[0].SourceCount).IsEqualTo(4);
            await Assert.That(results[0].CompleteSourceCount).IsEqualTo(2);
            await Assert.That(results[0].SpeedBytesPerSecond)
                .IsEqualTo(2 * 1024 * 1024);
            await Assert.That(results[0].AvailabilityPercent).IsEqualTo(75);
        }
        finally
        {
            listener.Stop();
            await serverTask;
        }
    }

}
