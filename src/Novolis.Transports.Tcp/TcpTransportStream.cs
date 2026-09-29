namespace Novolis.Transports.Tcp;

/// <summary>Adapts one TCP network stream to the common transport contract.</summary>
public sealed class TcpTransportStream : ITransportStream
{
    private readonly Stream _stream;
    private int _disposed;

    internal TcpTransportStream(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    /// <inheritdoc />
    public Stream Stream => _stream;

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            return _stream.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}
