using System;

namespace Novolis.Transports.Torrent.Exceptions;

/// <summary>
///     The TorrentInfoException.
/// </summary>
public class TorrentInfoException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TorrentInfoException" /> class.
    /// </summary>
    public TorrentInfoException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TorrentInfoException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TorrentInfoException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TorrentInfoException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public TorrentInfoException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}