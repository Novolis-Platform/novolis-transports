namespace Novolis.Transports;

/// <summary>Wire protocol represented by a transport implementation.</summary>
public enum TransportKind
{
    /// <summary>Transmission Control Protocol.</summary>
    Tcp,

    /// <summary>User Datagram Protocol.</summary>
    Udp,

    /// <summary>QUIC over UDP.</summary>
    Quic,
}
