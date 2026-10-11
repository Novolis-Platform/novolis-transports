namespace Novolis.Transports.NetworkPath;

/// <summary>Describes how an adapter relates to the selected path.</summary>
public enum NetworkPathKind
{
    Unknown,
    DefaultEgress,
    TunnelCandidate,
    Standby,
    Disconnected
}
