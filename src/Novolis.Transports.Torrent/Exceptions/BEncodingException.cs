using System;

namespace Novolis.Transports.Torrent.Exceptions;

/// <summary>
///     The BEncodingException.
/// </summary>
public class BEncodingException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BEncodingException" /> class.
    /// </summary>
    public BEncodingException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BEncodingException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BEncodingException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BEncodingException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public BEncodingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}