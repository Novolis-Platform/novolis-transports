using System;
using DefensiveProgrammingFramework;

namespace Novolis.Transports.Torrent.TorrentEventArgs;

/// <summary>
///     The torrent hashing event arguments.
/// </summary>
public sealed class TorrentHashingEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TorrentHashingEventArgs" /> class.
    /// </summary>
    /// <param name="torrentInfo">The torrent information.</param>
    public TorrentHashingEventArgs(TorrentInfo torrentInfo)
    {
        torrentInfo.CannotBeNull();

        this.TorrentInfo = torrentInfo;
    }

    /// <summary>
    ///     Prevents a default instance of the <see cref="TorrentHashingEventArgs" /> class from being created.
    /// </summary>
    private TorrentHashingEventArgs()
    {
    }

    /// <summary>
    ///     Gets the torrent information.
    /// </summary>
    /// <value>
    ///     The torrent information.
    /// </value>
    public TorrentInfo TorrentInfo { get; private set; }
}
