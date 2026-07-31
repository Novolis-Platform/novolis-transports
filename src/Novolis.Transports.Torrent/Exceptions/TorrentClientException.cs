using System;

namespace Novolis.Transports.Torrent.Exceptions;

/// <summary>
///     The TorrentClientException.
/// </summary>
public class TorrentClientException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TorrentClientException" /> class.
    /// </summary>
    public TorrentClientException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TorrentClientException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TorrentClientException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TorrentClientException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public TorrentClientException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}