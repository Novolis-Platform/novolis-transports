namespace Novolis.Transports.LocalIpc;

/// <summary>Accepts incoming local IPC connections.</summary>
public interface ILocalIpcListener : IAsyncDisposable
{
    ValueTask<ILocalIpcConnection> AcceptAsync(CancellationToken cancellationToken = default);
}
