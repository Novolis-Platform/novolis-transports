namespace Novolis.Transports.Discovery;

/// <summary>Product-neutral UDP discovery payload.</summary>
public sealed record DiscoveryBeacon(
    string ApplicationId,
    string ProtocolVersion,
    string HostName,
    string[] Endpoints);
