namespace Novolis.Transports.LocalIpc;

/// <summary>Duplex local IPC connection that sends and receives framed payloads.</summary>
public interface ILocalIpcConnection : IAsyncDisposable
{
    ValueTask SendAsync(LocalIpcFrame frame, CancellationToken cancellationToken = default);

    IAsyncEnumerable<LocalIpcFrame> ReadAllAsync(CancellationToken cancellationToken = default);
}
