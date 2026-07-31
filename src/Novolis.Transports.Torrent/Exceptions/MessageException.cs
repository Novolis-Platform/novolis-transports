using System;

namespace Novolis.Transports.Torrent.Exceptions;

/// <summary>
///     The MessageException.
/// </summary>
public class MessageException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageException" /> class.
    /// </summary>
    public MessageException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MessageException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MessageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}