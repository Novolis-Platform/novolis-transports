namespace Novolis.Transports.LocalIpc;

/// <summary>Connects to a named-pipe or Unix-domain-socket endpoint.</summary>
public interface ILocalIpcClient
{
    ValueTask<ILocalIpcConnection> ConnectAsync(LocalIpcEndpoint endpoint, CancellationToken cancellationToken = default);
}
