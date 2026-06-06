namespace Novolis.Transports.LocalIpc;

/// <summary>Declares a local IPC endpoint by address and transport kind.</summary>
public sealed record LocalIpcEndpoint(
    string Address,
    LocalIpcTransportKind Kind = LocalIpcTransportKind.Auto);
