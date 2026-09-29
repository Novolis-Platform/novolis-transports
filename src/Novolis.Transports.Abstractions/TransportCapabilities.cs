namespace Novolis.Transports;

/// <summary>Capabilities exposed by a transport connection.</summary>
[Flags]
public enum TransportCapabilities
{
    /// <summary>No capabilities were negotiated.</summary>
    None = 0,

    /// <summary>The connection can carry reliable ordered byte streams.</summary>
    ReliableOrderedStream = 1 << 0,

    /// <summary>The connection can carry unordered loss-tolerant datagrams.</summary>
    UnreliableDatagram = 1 << 1,

    /// <summary>The transport authenticates and encrypts the connection.</summary>
    Secure = 1 << 2,

    /// <summary>The connection can multiplex more than one stream.</summary>
    Multiplexed = 1 << 3,

    /// <summary>The transport can migrate a connection between network paths.</summary>
    ConnectionMigration = 1 << 4,
}
