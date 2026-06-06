namespace Novolis.Transports.LocalIpc;

/// <summary>Underlying local transport selected for the IPC endpoint.</summary>
public enum LocalIpcTransportKind
{
    Auto = 0,
    NamedPipe = 1,
    UnixDomainSocket = 2,
}
