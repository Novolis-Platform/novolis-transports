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

        using var client = new TcpClient { NoDelay = true };
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

        var requestWriter = new Ed2kBinaryWriter();
        requestWriter.WriteBytes(fileId.Bytes);
        await WriteAsync(
                stream,
                new Ed2kFrame(Ed2kProtocol.EmuleProtocol, Ed2kProtocol.PeerFileRequest, requestWriter.ToArray())
                    .EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);

        await using var output = File.Create(destinationPath);
        while (true)
        {
            var frame = await Ed2kFrameCodec.ReadTcpAsync(stream, cancellationToken)
                .ConfigureAwait(false);
            if (frame is null)
                throw new EndOfStreamException("The eD2k peer closed the transfer.");
            if (frame.Opcode == Ed2kProtocol.PeerHelloAnswer)
                continue;
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
}

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
    }

    public IPEndPoint LocalEndpoint =>
        (IPEndPoint)(this.listener.LocalEndpoint
                     ?? throw new InvalidOperationException("Start the peer host first."));

    public Ed2kCredits Credits { get; } = new();

    public void Share(Ed2kFileId fileId, ReadOnlySpan<byte> data)
    {
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
        if (request is null || request.Payload.Length < 16)
            return;
        var fileId = new Ed2kFileId(request.Payload.AsSpan(0, 16));
        if (!this.files.TryGetValue(fileId, out var data))
            return;

        var partWriter = new Ed2kBinaryWriter();
        partWriter.WriteBytes(fileId.Bytes);
        partWriter.WriteUInt32(0);
        partWriter.WriteUInt32(checked((uint)Math.Max(0, data.Length - 1)));
        partWriter.WriteBytes(data);
        await stream.WriteAsync(
                new Ed2kFrame(
                        Ed2kProtocol.EmuleProtocol,
                        Ed2kProtocol.PeerSendingPart,
                        partWriter.ToArray())
                    .EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        this.Credits.RecordUpload(peerHash, data.Length);
    }
}
