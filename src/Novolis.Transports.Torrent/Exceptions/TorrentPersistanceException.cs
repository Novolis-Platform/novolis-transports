using System;

namespace Novolis.Transports.Torrent.Exceptions;

/// <summary>
///     The TorrentPersistanceException.
/// </summary>
public class TorrentPersistanceException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TorrentPersistanceException" /> class.
    /// </summary>
    public TorrentPersistanceException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TorrentPersistanceException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TorrentPersistanceException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TorrentPersistanceException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public TorrentPersistanceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}