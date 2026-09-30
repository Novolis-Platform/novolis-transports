using System.Net;
using System.Net.Sockets;
using System.IO.Pipes;

namespace Novolis.Transports.LocalIpc;

internal sealed class LocalIpcConnection : ILocalIpcConnection
{
    private readonly Stream _stream;
    private readonly SemaphoreSlim _writeGate = new(1, 1);
    private bool _disposed;

    public LocalIpcConnection(Stream stream) => _stream = stream;

    public async ValueTask SendAsync(LocalIpcFrame frame, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _writeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await LocalIpcFrameCodec.WriteAsync(_stream, frame, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public async IAsyncEnumerable<LocalIpcFrame> ReadAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        while (true)
        {
            var frame = await LocalIpcFrameCodec.ReadAsync(_stream, cancellationToken).ConfigureAwait(false);
            if (frame is null)
                yield break;

            yield return frame;
        }
    }

    public ValueTask DisposeAsync()
    {
        if (_disposed)
            return ValueTask.CompletedTask;

        _disposed = true;
        _writeGate.Dispose();
        return _stream.DisposeAsync();
    }
}
