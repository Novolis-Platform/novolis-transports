using System.Net;
using System.Net.Sockets;

namespace Novolis.Transports.Ed2k;

/// <summary>
/// A small eD2k client session for server search and source discovery.
/// </summary>
public sealed class Ed2kSession : IAsyncDisposable, IDisposable
{
    private readonly string clientName;
    private readonly byte[] userHash;
    private TcpClient? serverClient;
    private NetworkStream? serverStream;

    public Ed2kSession(int listenPort = 4662, string clientName = "Novolis", byte[]? userHash = null)
    {
        if (listenPort is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(listenPort));
        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);
        if (userHash is not null && userHash.Length != 16)
            throw new ArgumentException("The eD2k user hash must contain 16 bytes.", nameof(userHash));

        this.ListenPort = listenPort;
        this.clientName = clientName;
        this.userHash = userHash?.ToArray() ?? Ed2kMessages.CreateUserHash();
    }

    public int ListenPort { get; }

    public ReadOnlyMemory<byte> UserHash => this.userHash;

    public Ed2kConnectionInfo? Connection { get; private set; }

    public async Task<Ed2kConnectionInfo> ConnectServerAsync(
        Ed2kServerEndpoint endpoint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (endpoint.Port is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(endpoint));

        await this.DisposeServerAsync().ConfigureAwait(false);
        var client = new TcpClient
        {
            NoDelay = true
        };
        await client.ConnectAsync(endpoint.Host, endpoint.Port, cancellationToken)
            .ConfigureAwait(false);
        var stream = client.GetStream();
        await WriteAsync(
                stream,
                Ed2kMessages.CreateLogin(this.userHash, this.ListenPort, this.clientName)
                    .EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);

        uint clientId = 0;
        var assignedPort = 0;
        while (true)
        {
            var frame = await Ed2kFrameCodec.ReadTcpAsync(stream, cancellationToken)
                .ConfigureAwait(false);
            if (frame is null)
                throw new EndOfStreamException("The eD2k server closed the login session.");
            if (Ed2kMessages.TryReadIdChange(frame, out clientId, out assignedPort))
                break;
            if (frame.Opcode == Ed2kProtocol.ServerMessage)
                continue;
        }

        this.serverClient = client;
        this.serverStream = stream;
        this.Connection = new Ed2kConnectionInfo(
            endpoint,
            clientId,
            Ed2kMessages.IsHighId(clientId),
            assignedPort == 0 ? this.ListenPort : assignedPort);
        return this.Connection;
    }

    public async Task<IReadOnlyList<Ed2kSearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var stream = this.RequireServerStream();
        await WriteAsync(
                stream,
                Ed2kMessages.CreateSearchRequest(query).EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);

        while (true)
        {
            var frame = await Ed2kFrameCodec.ReadTcpAsync(stream, cancellationToken)
                .ConfigureAwait(false);
            if (frame is null)
                throw new EndOfStreamException("The eD2k server closed the search session.");
            if (frame.Opcode == Ed2kProtocol.SearchResults)
                return Ed2kMessages.ParseSearchResults(frame.Payload);
        }
    }

    public async Task<IReadOnlyList<Ed2kSource>> GetSourcesAsync(
        Ed2kFileId fileId,
        CancellationToken cancellationToken = default)
    {
        var stream = this.RequireServerStream();
        await WriteAsync(
                stream,
                Ed2kMessages.CreateSourceRequest(fileId).EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);

        while (true)
        {
            var frame = await Ed2kFrameCodec.ReadTcpAsync(stream, cancellationToken)
                .ConfigureAwait(false);
            if (frame is null)
                throw new EndOfStreamException("The eD2k server closed the source session.");
            if (frame.Opcode == Ed2kProtocol.Sources)
                return Ed2kMessages.ParseSources(frame.Payload);
        }
    }

    public async Task DownloadAsync(
        Ed2kSearchResult result,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        if (!Ed2kMessages.IsHighId(result.ClientId))
            throw new NotSupportedException("Downloading from a LowID source requires callback support.");

        var address = new IPAddress(
        [
            (byte)(result.ClientId & 0xFF),
            (byte)((result.ClientId >> 8) & 0xFF),
            (byte)((result.ClientId >> 16) & 0xFF),
            (byte)((result.ClientId >> 24) & 0xFF)
        ]);
        using var peer = new TcpClient();
        await peer.ConnectAsync(address, result.Port, cancellationToken).ConfigureAwait(false);
        await using var stream = peer.GetStream();

        var helloWriter = new Ed2kBinaryWriter();
        helloWriter.WriteBytes(this.userHash);
        helloWriter.WriteUInt16(checked((ushort)this.ListenPort));
        helloWriter.WriteBytes(Ed2kTagCodec.Encode(
        [
            new Ed2kTag(Ed2kTagIds.Name, this.clientName),
            new Ed2kTag(Ed2kTagIds.ClientVersion, 0x3Cu)
        ]));
        await WriteAsync(
                stream,
                new Ed2kFrame(Ed2kProtocol.EmuleProtocol, Ed2kProtocol.PeerHello, helloWriter.ToArray())
                    .EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);

        var requestWriter = new Ed2kBinaryWriter();
        requestWriter.WriteBytes(result.FileId.Bytes);
        await WriteAsync(
                stream,
                new Ed2kFrame(Ed2kProtocol.EmuleProtocol, Ed2kProtocol.PeerFileRequest, requestWriter.ToArray())
                    .EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);

        await using var output = File.Create(destinationPath);
        var frame = await Ed2kFrameCodec.ReadTcpAsync(stream, cancellationToken)
            .ConfigureAwait(false);
        if (frame is null || frame.Opcode != Ed2kProtocol.PeerSendingPart)
            throw new InvalidDataException("The eD2k peer did not send a part.");
        var reader = new Ed2kBinaryReader(frame.Payload);
        _ = reader.ReadBytes(16);
        var start = reader.ReadUInt32();
        var end = reader.ReadUInt32();
        var bytes = reader.ReadBytes(checked((int)(end - start + 1)));
        await output.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        this.serverStream?.Dispose();
        this.serverClient?.Dispose();
        this.serverStream = null;
        this.serverClient = null;
        this.Connection = null;
    }

    public async ValueTask DisposeAsync()
    {
        await this.DisposeServerAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private static async Task WriteAsync(
        NetworkStream stream,
        byte[] data,
        CancellationToken cancellationToken)
    {
        await stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private NetworkStream RequireServerStream()
    {
        return this.serverStream
               ?? throw new InvalidOperationException("Connect to an eD2k server first.");
    }

    private async ValueTask DisposeServerAsync()
    {
        if (this.serverStream is not null)
            await this.serverStream.DisposeAsync().ConfigureAwait(false);
        this.serverClient?.Dispose();
        this.serverStream = null;
        this.serverClient = null;
        this.Connection = null;
    }
}
