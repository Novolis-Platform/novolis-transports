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
        var client = new TcpClient(AddressFamily.InterNetwork)
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
            {
                var payload = DecodePayload(frame);
                return Ed2kMessages.ParseSearchResults(payload);
            }
        }
    }

    public async Task OfferFilesAsync(
        IReadOnlyList<Ed2kSharedFile> files,
        CancellationToken cancellationToken = default)
    {
        var stream = this.RequireServerStream();
        await WriteAsync(
                stream,
                Ed2kMessages.CreateOfferFiles(files).EncodeTcp(),
                cancellationToken)
            .ConfigureAwait(false);
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
            {
                var payload = DecodePayload(frame);
                return Ed2kMessages.ParseSources(payload);
            }
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
        await Ed2kPeerSession.DownloadAsync(
                new IPEndPoint(address, result.Port),
                result.FileId,
                result.Size,
                destinationPath,
                this.userHash,
                this.ListenPort,
                cancellationToken)
            .ConfigureAwait(false);
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

    private static byte[] DecodePayload(Ed2kFrame frame)
    {
        return frame.Protocol == Ed2kProtocol.ZlibProtocol
            ? Ed2kCompression.Decompress(frame.Payload)
            : frame.Payload;
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
