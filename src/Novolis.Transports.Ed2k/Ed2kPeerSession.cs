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
        if (expectedSize is <= 0 or > uint.MaxValue)
        {
            throw new NotSupportedException(
                "The eD2k multipart MVP supports files from 1 byte through 4 GiB.");
        }

        if (File.Exists(destinationPath))
            File.Delete(destinationPath);
        await using (var output = new FileStream(
                         destinationPath,
                         FileMode.Create,
                         FileAccess.Write,
                         FileShare.Read,
                         81920,
                         FileOptions.Asynchronous))
        {
            output.SetLength(expectedSize);
        }

        for (long offset = 0; offset < expectedSize; offset += Ed2kProtocol.PartSize)
        {
            var partSize = checked((int)Math.Min(
                Ed2kProtocol.PartSize,
                expectedSize - offset));
            await DownloadPartAsync(
                    endpoint,
                    fileId,
                    offset,
                    partSize,
                    destinationPath,
                    userHash,
                    listenPort,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        await using var verification = File.OpenRead(destinationPath);
        if (verification.Length != expectedSize
            || await Ed2kFileId.ComputeAsync(verification, cancellationToken)
                .ConfigureAwait(false) != fileId)
        {
            throw new InvalidDataException("The downloaded file failed its eD2k file-id check.");
        }
    }

    /// <summary>
    /// Downloads one requested eD2k part into a preallocated destination.
    /// </summary>
    public static async Task DownloadPartAsync(
        IPEndPoint endpoint,
        Ed2kFileId fileId,
        long startOffset,
        int partSize,
        string destinationPath,
        ReadOnlyMemory<byte> userHash,
        int listenPort = 4662,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        if (userHash.Length != 16)
            throw new ArgumentException("The eD2k user hash must contain 16 bytes.", nameof(userHash));
        if (startOffset < 0 || startOffset > uint.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(startOffset));
        if (partSize is <= 0 or > Ed2kProtocol.PartSize)
            throw new ArgumentOutOfRangeException(nameof(partSize));
        var endOffset = checked(startOffset + partSize - 1);
        if (endOffset > uint.MaxValue)
            throw new NotSupportedException(
                "The eD2k part range exceeds the 32-bit peer protocol.");

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
                new Ed2kFrame(
                        Ed2kProtocol.EmuleProtocol,
                        Ed2kProtocol.PeerHello,
                        helloWriter.ToArray())
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
                new Ed2kFrame(
                        Ed2kProtocol.EmuleProtocol,
                        Ed2kProtocol.PeerFileRequest,
                        requestWriter.ToArray())
                    .EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);

        var partsWriter = new Ed2kBinaryWriter();
        partsWriter.WriteBytes(fileId.Bytes);
        partsWriter.WriteUInt32(checked((uint)startOffset));
        partsWriter.WriteUInt32(0);
        partsWriter.WriteUInt32(0);
        partsWriter.WriteUInt32(checked((uint)endOffset));
        partsWriter.WriteUInt32(0);
        partsWriter.WriteUInt32(0);
        await WriteAsync(
                stream,
                new Ed2kFrame(
                        Ed2kProtocol.EmuleProtocol,
                        Ed2kProtocol.PeerRequestParts,
                        partsWriter.ToArray())
                    .EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);

        await using var output = new FileStream(
            destinationPath,
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.Read,
            81920,
            FileOptions.Asynchronous);
        while (true)
        {
            var frame = await Ed2kFrameCodec.ReadTcpAsync(stream, cancellationToken)
                .ConfigureAwait(false);
            if (frame is null)
                throw new EndOfStreamException("The eD2k peer closed the transfer.");
            if (frame.Opcode == Ed2kProtocol.PeerHelloAnswer
                || frame.Opcode == Ed2kProtocol.PeerFileStatus
                || frame.Opcode == Ed2kProtocol.PeerAcceptUploadReq)
            {
                continue;
            }
            if (frame.Opcode == Ed2kProtocol.PeerHashSetAnswer)
            {
                ValidateHashSet(frame.Payload, fileId);
                continue;
            }
            if (frame.Opcode != Ed2kProtocol.PeerSendingPart)
                continue;

            var reader = new Ed2kBinaryReader(frame.Payload);
            var receivedId = new Ed2kFileId(reader.ReadBytes(16));
            var receivedStart = reader.ReadUInt32();
            var receivedEnd = reader.ReadUInt32();
            if (receivedEnd < receivedStart)
                throw new InvalidDataException("The eD2k peer sent a reversed part range.");
            var receivedLength = checked((int)(receivedEnd - receivedStart + 1));
            var bytes = reader.ReadBytes(receivedLength);
            if (receivedId != fileId
                || receivedStart != startOffset
                || receivedLength != partSize)
            {
                throw new InvalidDataException("The eD2k peer sent an unexpected part.");
            }

            output.Position = startOffset;
            await output.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
            await output.FlushAsync(cancellationToken).ConfigureAwait(false);
            return;
        }
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
        Ed2kFileId fileId)
    {
        var reader = new Ed2kBinaryReader(payload);
        if (new Ed2kFileId(reader.ReadBytes(16)) != fileId)
            throw new InvalidDataException("The eD2k peer returned the wrong hash set.");
        var count = reader.ReadUInt32();
        if (count == 0 || count > reader.Remaining / 16)
            throw new InvalidDataException("The eD2k peer returned an invalid hash set.");
        for (var index = 0; index < count; index++)
            _ = reader.ReadBytes(16);
    }
}
