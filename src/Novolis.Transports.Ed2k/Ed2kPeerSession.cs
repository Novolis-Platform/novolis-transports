using System.Net;
using System.Net.Sockets;

namespace Novolis.Transports.Ed2k;

/// <summary>
/// Client and loopback-host helpers for the eD2k peer wire.
/// </summary>
public sealed class Ed2kPeerSession
{
    public static async Task DownloadAsync(
        IPEndPoint endpoint,
        Ed2kFileId fileId,
        long expectedSize,
        string destinationPath,
        ReadOnlyMemory<byte> userHash,
        int listenPort = 4662,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        if (userHash.Length != 16)
            throw new ArgumentException("The eD2k user hash must contain 16 bytes.", nameof(userHash));
        if (expectedSize is < 0 or > Ed2kProtocol.PartSize)
            throw new NotSupportedException("The first peer client supports one eD2k part per transfer.");

        using var client = new TcpClient(endpoint.AddressFamily) { NoDelay = true };
        await client.ConnectAsync(endpoint.Address, endpoint.Port, cancellationToken)
            .ConfigureAwait(false);
        await using var stream = client.GetStream();

        var helloWriter = new Ed2kBinaryWriter();
        helloWriter.WriteBytes(userHash.Span);
        helloWriter.WriteUInt16(checked((ushort)listenPort));
        helloWriter.WriteBytes(Ed2kTagCodec.Encode(
        [
            new Ed2kTag(Ed2kTagIds.Name, "Novolis"),
            new Ed2kTag(Ed2kTagIds.ClientVersion, 0x3Cu)
        ]));
        await WriteAsync(
                stream,
                new Ed2kFrame(Ed2kProtocol.EmuleProtocol, Ed2kProtocol.PeerHello, helloWriter.ToArray())
                    .EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);

        var hashSetRequestWriter = new Ed2kBinaryWriter();
        hashSetRequestWriter.WriteBytes(fileId.Bytes);
        await WriteAsync(
                stream,
                new Ed2kFrame(
                        Ed2kProtocol.EmuleProtocol,
                        Ed2kProtocol.PeerHashSetRequest,
                        hashSetRequestWriter.ToArray())
                    .EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);

        var requestWriter = new Ed2kBinaryWriter();
        requestWriter.WriteBytes(fileId.Bytes);
        await WriteAsync(
                stream,
                new Ed2kFrame(Ed2kProtocol.EmuleProtocol, Ed2kProtocol.PeerFileRequest, requestWriter.ToArray())
                    .EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);

        await using (var output = File.Create(destinationPath))
        {
            while (true)
            {
                var frame = await Ed2kFrameCodec.ReadTcpAsync(stream, cancellationToken)
                    .ConfigureAwait(false);
                if (frame is null)
                    throw new EndOfStreamException("The eD2k peer closed the transfer.");
                if (frame.Opcode == Ed2kProtocol.PeerHelloAnswer)
                    continue;
                if (frame.Opcode == Ed2kProtocol.PeerHashSetAnswer)
                {
                    ValidateHashSet(frame.Payload, fileId, expectedSize);
                    continue;
                }
                if (frame.Opcode != Ed2kProtocol.PeerSendingPart)
                    continue;

                var reader = new Ed2kBinaryReader(frame.Payload);
                var receivedId = new Ed2kFileId(reader.ReadBytes(16));
                var start = reader.ReadUInt32();
                var end = reader.ReadUInt32();
                var length = checked((int)(end - start + 1));
                var bytes = reader.ReadBytes(length);
                if (receivedId != fileId || start != 0 || length != expectedSize)
                    throw new InvalidDataException("The eD2k peer sent an unexpected part.");

                await output.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
                await output.FlushAsync(cancellationToken).ConfigureAwait(false);
                break;
            }
        }

        await using var verification = File.OpenRead(destinationPath);
        if (verification.Length != expectedSize || Ed2kFileId.Compute(verification) != fileId)
            throw new InvalidDataException("The downloaded part failed its eD2k file-id check.");
    }

    private static async Task WriteAsync(
        NetworkStream stream,
        byte[] data,
        CancellationToken cancellationToken)
    {
        await stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateHashSet(
        ReadOnlySpan<byte> payload,
        Ed2kFileId fileId,
        long expectedSize)
    {
        var reader = new Ed2kBinaryReader(payload);
        if (new Ed2kFileId(reader.ReadBytes(16)) != fileId)
            throw new InvalidDataException("The eD2k peer returned the wrong hash set.");
        if (reader.ReadUInt32() != 1 || reader.Remaining < 16)
            throw new InvalidDataException("The eD2k peer returned an unsupported hash set.");
        _ = new Ed2kFileId(reader.ReadBytes(16));
        if (expectedSize > Ed2kProtocol.PartSize)
            throw new NotSupportedException("The first peer client supports one eD2k part per transfer.");
    }
}
