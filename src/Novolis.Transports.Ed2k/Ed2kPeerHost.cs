using System.Net;
using System.Net.Sockets;

namespace Novolis.Transports.Ed2k;

/// <summary>
/// A minimal inbound peer used by tests and local dogfood.
/// </summary>
public sealed class Ed2kPeerHost : IAsyncDisposable
{
    private readonly TcpListener listener;
    private readonly Dictionary<Ed2kFileId, byte[]> files = new();
    private readonly byte[] userHash = Ed2kMessages.CreateUserHash();
    private readonly CancellationTokenSource shutdown = new();
    private Task? acceptLoop;

    public Ed2kPeerHost(int port = 0, IPAddress? address = null)
    {
        if (port is < 0 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(port));
        this.listener = new TcpListener(address ?? IPAddress.Loopback, port);
        this.Credits = new Ed2kCredits();
        this.UploadQueue = new Ed2kUploadQueue(this.Credits);
    }

    public IPEndPoint LocalEndpoint =>
        (IPEndPoint)(this.listener.LocalEndpoint
                     ?? throw new InvalidOperationException("Start the peer host first."));

    public Ed2kCredits Credits { get; }

    public Ed2kUploadQueue UploadQueue { get; }

    public void Share(Ed2kFileId fileId, ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
            throw new ArgumentException("An eD2k peer share must contain at least one byte.", nameof(data));
        if (Ed2kFileId.Compute(data) != fileId)
            throw new ArgumentException("The file id does not match the shared bytes.", nameof(fileId));
        this.files[fileId] = data.ToArray();
    }

    public Task StartAsync()
    {
        this.listener.Start();
        this.acceptLoop = AcceptLoopAsync(this.shutdown.Token);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        this.shutdown.Cancel();
        this.listener.Stop();
        if (this.acceptLoop is not null)
        {
            try
            {
                await this.acceptLoop.ConfigureAwait(false);
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        this.shutdown.Dispose();
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await this.listener.AcceptTcpClientAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (SocketException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            _ = HandleClientAsync(client, cancellationToken);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using var clientLifetime = client;
        await using var stream = client.GetStream();
        var hello = await Ed2kFrameCodec.ReadTcpAsync(stream, cancellationToken).ConfigureAwait(false);
        if (hello is null || hello.Payload.Length < 16)
            return;

        var peerHash = hello.Payload.AsSpan(0, 16).ToArray();
        var answerWriter = new Ed2kBinaryWriter();
        answerWriter.WriteBytes(this.userHash);
        answerWriter.WriteUInt16(0);
        answerWriter.WriteBytes(Ed2kTagCodec.Encode(
        [
            new Ed2kTag(Ed2kTagIds.Name, "Novolis peer"),
            new Ed2kTag(Ed2kTagIds.ClientVersion, 0x3Cu)
        ]));
        await stream.WriteAsync(
                new Ed2kFrame(
                        Ed2kProtocol.EmuleProtocol,
                        Ed2kProtocol.PeerHelloAnswer,
                        answerWriter.ToArray())
                    .EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

        var request = await Ed2kFrameCodec.ReadTcpAsync(stream, cancellationToken).ConfigureAwait(false);
        while (request is not null && request.Opcode == Ed2kProtocol.PeerHashSetRequest)
        {
            if (request.Payload.Length < 16)
                return;

            var hashSetFileId = new Ed2kFileId(request.Payload.AsSpan(0, 16));
            if (this.files.TryGetValue(hashSetFileId, out var hashSetData))
            {
                var hashSetWriter = new Ed2kBinaryWriter();
                hashSetWriter.WriteBytes(hashSetFileId.Bytes);
                var hashCount = checked((uint)(
                    ((long)hashSetData.Length + Ed2kProtocol.PartSize - 1)
                    / Ed2kProtocol.PartSize));
                hashSetWriter.WriteUInt32(hashCount);
                for (var offset = 0; offset < hashSetData.Length; offset += Ed2kProtocol.PartSize)
                {
                    var length = Math.Min(
                        Ed2kProtocol.PartSize,
                        hashSetData.Length - offset);
                    hashSetWriter.WriteBytes(
                        Ed2kMd4.Compute(hashSetData.AsSpan(offset, length)));
                }
                await stream.WriteAsync(
                        new Ed2kFrame(
                                Ed2kProtocol.EmuleProtocol,
                                Ed2kProtocol.PeerHashSetAnswer,
                                hashSetWriter.ToArray())
                            .EncodeTcp(),
                        cancellationToken)
                    .ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            request = await Ed2kFrameCodec.ReadTcpAsync(stream, cancellationToken)
                .ConfigureAwait(false);
        }

        if (request is null || request.Opcode != Ed2kProtocol.PeerFileRequest ||
            request.Payload.Length < 16)
            return;
        var fileId = new Ed2kFileId(request.Payload.AsSpan(0, 16));
        if (!this.files.TryGetValue(fileId, out var data))
            return;

        var partRequest = await Ed2kFrameCodec.ReadTcpAsync(stream, cancellationToken)
            .ConfigureAwait(false);
        var start = 0;
        var end = Math.Min(Ed2kProtocol.PartSize, data.Length) - 1;
        if (partRequest?.Opcode == Ed2kProtocol.PeerRequestParts)
        {
            if (partRequest.Payload.Length < 16 + (6 * sizeof(uint)))
                return;
            var requestedFileId = new Ed2kFileId(
                partRequest.Payload.AsSpan(0, 16));
            if (requestedFileId != fileId)
                return;

            var partReader = new Ed2kBinaryReader(partRequest.Payload.AsSpan(16));
            start = checked((int)partReader.ReadUInt32());
            _ = partReader.ReadUInt32();
            _ = partReader.ReadUInt32();
            end = checked((int)partReader.ReadUInt32());
            if (start < 0
                || end < start
                || end >= data.Length
                || end - start + 1 > Ed2kProtocol.PartSize)
            {
                return;
            }
        }

        this.UploadQueue.Enqueue(peerHash);
        if (!this.UploadQueue.TryTakeNext(DateTimeOffset.UtcNow, out _))
            return;

        var partWriter = new Ed2kBinaryWriter();
        partWriter.WriteBytes(fileId.Bytes);
        partWriter.WriteUInt32(checked((uint)start));
        partWriter.WriteUInt32(checked((uint)end));
        partWriter.WriteBytes(data.AsSpan(start, end - start + 1));
        await stream.WriteAsync(
                new Ed2kFrame(
                        Ed2kProtocol.EmuleProtocol,
                        Ed2kProtocol.PeerSendingPart,
                        partWriter.ToArray())
                    .EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        this.Credits.RecordUpload(peerHash, end - start + 1);
    }
}
