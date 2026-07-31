using System;
using DefensiveProgrammingFramework;

namespace Novolis.Transports.Torrent.PeerWireProtocol.TorrentEventArgs;

/// <summary>
///     The piece requested event arguments.
/// </summary>
public sealed class PieceRequestedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PieceRequestedEventArgs" /> class.
    /// </summary>
    /// <param name="pieceIndex">Index of the piece.</param>
    public PieceRequestedEventArgs(int pieceIndex)
    {
        pieceIndex.MustBeGreaterThanOrEqualTo(0);

        this.PieceIndex = pieceIndex;
    }

    /// <summary>
    ///     Gets or sets the piece data.
    /// </summary>
    /// <value>
    ///     The piece data.
    /// </value>
    public byte[] PieceData { get; set; }

    /// <summary>
    ///     Gets the index of the piece.
    /// </summary>
    /// <value>
    ///     The index of the piece.
    /// </value>
    public int PieceIndex { get; private set; }
}
