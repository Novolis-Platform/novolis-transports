using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using DefensiveProgrammingFramework;
using Novolis.Transports.Torrent.Extensions;
using Novolis.Transports.Torrent.PeerWireProtocol.Messages;
using Novolis.Transports.Torrent.TrackerProtocol.Udp.Messages;

namespace Novolis.Transports.Torrent.TrackerProtocol.Http.Messages;

/// <summary>
///     The announce message.
/// </summary>
public class AnnounceMessage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AnnounceMessage" /> class.
    /// </summary>
    /// <param name="infohash">The info hash.</param>
    /// <param name="peerId">The peer unique identifier.</param>
    /// <param name="port">The port.</param>
    /// <param name="bytesUploaded">The bytes uploaded.</param>
    /// <param name="bytesDownloaded">The bytes downloaded.</param>
    /// <param name="bytesLeft">The bytes left.</param>
    /// <param name="peersWantedCount">The peers wanted count.</param>
    /// <param name="trackingEvent">The tracking event.</param>
    public AnnounceMessage(string infohash, string peerId, int port, long bytesUploaded, long bytesDownloaded,
        long bytesLeft, int peersWantedCount, TrackingEvent trackingEvent)
    {
        infohash.CannotBeNullOrEmpty();
        infohash.Length.MustBeEqualTo(40);
        peerId.CannotBeNullOrEmpty();
        peerId.Length.MustBeGreaterThanOrEqualTo(20);
        port.MustBeGreaterThanOrEqualTo(IPEndPoint.MinPort);
        port.MustBeLessThanOrEqualTo(IPEndPoint.MaxPort);
        bytesUploaded.MustBeGreaterThanOrEqualTo(0);
        bytesDownloaded.MustBeGreaterThanOrEqualTo(0);
        bytesLeft.MustBeGreaterThanOrEqualTo(0);
        peersWantedCount.MustBeGreaterThanOrEqualTo(0);

        this.BytesDownloaded = bytesDownloaded;
        this.BytesUploaded = bytesUploaded;
        this.BytesLeft = bytesLeft;
        this.TrackingEvent = trackingEvent;
        this.InfoHash = infohash;
        this.PeerId = peerId;
        this.Port = port;
        this.PeersWantedCount = peersWantedCount;
    }

    /// <summary>
    ///     Prevents a default instance of the <see cref="AnnounceMessage" /> class from being created.
    /// </summary>
    private AnnounceMessage()
    {
    }

    /// <summary>
    ///     Gets the bytes downloaded.
    /// </summary>
    /// <value>
    ///     The bytes downloaded.
    /// </value>
    public long BytesDownloaded { get; }

    /// <summary>
    ///     Gets the bytes left.
    /// </summary>
    /// <value>
    ///     The bytes left.
    /// </value>
    public long BytesLeft { get; }

    /// <summary>
    ///     Gets the bytes uploaded.
    /// </summary>
    /// <value>
    ///     The bytes uploaded.
    /// </value>
    public long BytesUploaded { get; }

    /// <summary>
    ///     Gets the information hash.
    /// </summary>
    /// <value>
    ///     The information hash.
    /// </value>
    public string InfoHash { get; }

    /// <summary>
    ///     Gets the peer unique identifier.
    /// </summary>
    /// <value>
    ///     The peer unique identifier.
    /// </value>
    public string PeerId { get; }

    /// <summary>
    ///     Gets the peers wanted count.
    /// </summary>
    /// <value>
    ///     The peers wanted count.
    /// </value>
    public int PeersWantedCount { get; }

    /// <summary>
    ///     Gets the port.
    /// </summary>
    /// <value>
    ///     The port.
    /// </value>
    public int Port { get; }

    /// <summary>
    ///     Gets the tracking event.
    /// </summary>
    /// <value>
    ///     The tracking event.
    /// </value>
    public TrackingEvent TrackingEvent { get; }

    /// <summary>
    ///     Encodes the message.
    /// </summary>
    /// <returns>The encoded message.</returns>
    public string Encode()
    {
        // BitTorrent HTTP announce requires percent-encoded 20-byte info_hash and peer_id.
        // Do not put raw binary into the query string (HttpUtility.UrlEncode(string) after ASCII.GetString breaks peer_id).
        var infoHash = UrlEncodeBytes(this.InfoHash.ToByteArray());
        var peerId = UrlEncodeBytes(Message.FromPeerId(this.PeerId));

        return string.Join("&",
            $"info_hash={infoHash}",
            $"peer_id={peerId}",
            $"port={this.Port}",
            $"uploaded={this.BytesUploaded}",
            $"downloaded={this.BytesDownloaded}",
            $"left={this.BytesLeft}",
            $"numwant={this.PeersWantedCount}",
            "compact=1",
            $"event={this.TrackingEvent.ToString().ToLower(CultureInfo.InvariantCulture)}");
    }

    static string UrlEncodeBytes(byte[] bytes) =>
        string.Concat(bytes.Select(b => "%" + b.ToString("X2", CultureInfo.InvariantCulture)));
}
