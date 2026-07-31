using System;

namespace Novolis.Transports.Torrent.Exceptions;

/// <summary>
///     The PeerWireProtocolException.
/// </summary>
public class PeerWireProtocolException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PeerWireProtocolException" /> class.
    /// </summary>
    public PeerWireProtocolException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PeerWireProtocolException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PeerWireProtocolException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PeerWireProtocolException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public PeerWireProtocolException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}