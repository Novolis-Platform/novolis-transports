using System.Net.Quic;
using System.Runtime.Versioning;

namespace Novolis.Transports.Quic;

/// <summary>Adapts one QUIC stream to the common transport contract.</summary>
[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class QuicTransportStream : ITransportStream
{
    private readonly QuicStream _stream;
    private int _disposed;

    internal QuicTransportStream(QuicStream stream)
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
