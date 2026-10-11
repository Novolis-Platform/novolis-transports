using System.Net;
using System.Net.Sockets;
using Novolis.Transports.Ed2k;

namespace Novolis.Transports.Kad;

/// <summary>
/// A plain eMule Kad UDP node with bounded iterative lookup helpers.
/// </summary>
public sealed class KadNode : IAsyncDisposable, IDisposable
{
    private readonly UdpClient udp;

    public KadNode(
        int udpPort = 4672,
        int tcpPort = 4662,
        KadId? nodeId = null)
    {
        if (udpPort is < 0 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(udpPort));
        if (tcpPort is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(tcpPort));

        this.udp = new UdpClient(new IPEndPoint(IPAddress.Any, udpPort));
        this.NodeId = nodeId ?? KadId.CreateRandom();
        this.TcpPort = tcpPort;
        this.RoutingTable = new KadRoutingTable(this.NodeId);
    }

    public KadId NodeId { get; }

    public int TcpPort { get; }

    public IPEndPoint LocalEndpoint => (IPEndPoint)this.udp.Client.LocalEndPoint!;

    public KadRoutingTable RoutingTable { get; }

    public async Task<IReadOnlyList<KadContact>> BootstrapAsync(
        KadContact bootstrap,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bootstrap);
        var packet = new KadPacket(KadProtocol.Header, KadProtocol.BootstrapRequest, []);
        var response = await SendAndReceiveAsync(
                packet,
                bootstrap.Endpoint,
                cancellationToken)
            .ConfigureAwait(false);
        if (response.Opcode != KadProtocol.BootstrapResponse)
            throw new InvalidDataException($"Unexpected Kad bootstrap opcode 0x{response.Opcode:X2}.");

        var contacts = KadWireCodec.DecodeBootstrapResponse(response.Payload);
        foreach (var contact in contacts)
            this.RoutingTable.TryAdd(contact);
        return contacts;
    }

    public async Task<IReadOnlyList<KadSource>> FindSourcesAsync(
        Ed2kFileId fileId,
        KadContact contact,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contact);
        var payload = KadWireCodec.EncodeSearchRequest(this.NodeId, fileId);
        var response = await SendAndReceiveAsync(
                new KadPacket(KadProtocol.Header, KadProtocol.SearchSourceRequest, payload),
                contact.Endpoint,
                cancellationToken)
            .ConfigureAwait(false);
        if (response.Opcode != KadProtocol.SearchResponse)
            throw new InvalidDataException($"Unexpected Kad search opcode 0x{response.Opcode:X2}.");
        return KadWireCodec.DecodeSearchResponse(response.Payload);
    }

    public async Task PublishAsync(
        Ed2kFileId fileId,
        IReadOnlyList<KadSource> sources,
        KadContact contact,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(contact);
        var payload = KadWireCodec.EncodePublishSources(this.NodeId, fileId, sources);
        var response = await SendAndReceiveAsync(
                new KadPacket(KadProtocol.Header, KadProtocol.PublishSourceRequest, payload),
                contact.Endpoint,
                cancellationToken)
            .ConfigureAwait(false);
        if (response.Opcode is not (KadProtocol.PublishSourceResponse or KadProtocol.PublishSourceRequest))
            throw new InvalidDataException($"Unexpected Kad publish opcode 0x{response.Opcode:X2}.");
    }

    public void Dispose()
    {
        this.udp.Dispose();
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        this.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task<KadPacket> SendAndReceiveAsync(
        KadPacket packet,
        IPEndPoint endpoint,
        CancellationToken cancellationToken)
    {
        var bytes = packet.Encode();
        await this.udp.SendAsync(bytes, bytes.Length, endpoint).WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        var received = await this.udp.ReceiveAsync(cancellationToken).ConfigureAwait(false);
        return KadPacketCodec.Decode(received.Buffer);
    }
}
