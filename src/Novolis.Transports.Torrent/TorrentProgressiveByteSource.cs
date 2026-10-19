using Novolis.Transports.Torrent.PeerWireProtocol;
using Novolis.Video;

namespace Novolis.Transports.Torrent;

/// <summary>
/// Exposes one downloaded torrent file as a progressive random-access source.
/// </summary>
/// <remarks>
/// Reads wait for verified pieces and prioritize the requested playback window.
/// The source never returns sparse-file zeros as if they were media bytes.
/// </remarks>
public sealed class TorrentProgressiveByteSource : IProgressiveByteSource
{
    private readonly PersistenceManager _persistence;
    private readonly PieceManager _pieces;
    private readonly long _torrentOffset;
    private bool _disposed;

    /// <summary>
    /// Initializes a source over one contiguous file in a torrent.
    /// </summary>
    public TorrentProgressiveByteSource(
        PersistenceManager persistence,
        PieceManager pieces,
        long torrentOffset,
        long length)
    {
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _pieces = pieces ?? throw new ArgumentNullException(nameof(pieces));
        if (torrentOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(torrentOffset));
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length));
        if (torrentOffset > pieces.TorrentLength - length)
            throw new ArgumentOutOfRangeException(nameof(length));

        _torrentOffset = torrentOffset;
        Length = length;
    }

    /// <inheritdoc />
    public long Length { get; }

    /// <inheritdoc />
    public bool IsRangeAvailable(long offset, int length)
    {
        ValidateRange(offset, length);
        if (length == 0)
            return true;

        var firstPiece = GetPieceIndex(offset);
        var lastPiece = GetPieceIndex(offset + length - 1L);
        for (var pieceIndex = firstPiece; pieceIndex <= lastPiece; pieceIndex++)
            if (_pieces.BitField[pieceIndex] != PieceStatus.Present)
                return false;

        return true;
    }

    /// <inheritdoc />
    public void RequestRange(long offset, int length)
    {
        ValidateRange(offset, length);
        if (length == 0)
            return;

        var firstPiece = GetPieceIndex(offset);
        var lastPiece = GetPieceIndex(offset + length - 1L);
        for (var pieceIndex = firstPiece; pieceIndex <= lastPiece; pieceIndex++)
            _pieces.Prioritize(pieceIndex);
    }

    /// <inheritdoc />
    public async ValueTask<int> ReadAtAsync(
        long offset,
        Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        ValidateOffset(offset);
        if (destination.Length == 0 || offset >= Length)
            return 0;

        var count = (int)Math.Min(destination.Length, Length - offset);
        await WaitForRangeAsync(offset, count, cancellationToken).ConfigureAwait(false);

        var copied = 0;
        while (copied < count)
        {
            var sourceOffset = offset + copied;
            var torrentPosition = _torrentOffset + sourceOffset;
            var pieceIndex = (int)(torrentPosition / _pieces.PieceLength);
            var pieceOffset = (int)(torrentPosition % _pieces.PieceLength);
            var pieceData = _persistence.Get(pieceIndex);
            var copyLength = Math.Min(
                count - copied,
                pieceData.Length - pieceOffset);
            pieceData.AsMemory(pieceOffset, copyLength)
                .CopyTo(destination.Slice(copied, copyLength));
            copied += copyLength;
        }

        return copied;
    }

    /// <inheritdoc />
    public async ValueTask WaitForRangeAsync(
        long offset,
        int length,
        CancellationToken cancellationToken = default)
    {
        ValidateRange(offset, length);
        if (length == 0)
            return;

        RequestRange(offset, length);
        var firstPiece = GetPieceIndex(offset);
        var lastPiece = GetPieceIndex(offset + length - 1L);
        for (var pieceIndex = firstPiece; pieceIndex <= lastPiece; pieceIndex++)
            await _pieces.WaitForPieceAsync(pieceIndex, cancellationToken)
                .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _disposed = true;
        return ValueTask.CompletedTask;
    }

    private int GetPieceIndex(long sourceOffset) =>
        checked((int)((_torrentOffset + sourceOffset) / _pieces.PieceLength));

    private void ValidateOffset(long offset)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TorrentProgressiveByteSource));
        if (offset < 0 || offset > Length)
            throw new ArgumentOutOfRangeException(nameof(offset));
    }

    private void ValidateRange(long offset, int length)
    {
        ValidateOffset(offset);
        if (length < 0 || length > Length - offset)
            throw new ArgumentOutOfRangeException(nameof(length));
    }
}
