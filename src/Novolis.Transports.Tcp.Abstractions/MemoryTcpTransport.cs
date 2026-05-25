namespace Novolis.Transports.Tcp.Abstractions;

/// <summary>In-memory round-trip helper for TCP handler tests (no sockets).</summary>
public static class MemoryTcpTransport
{
    /// <summary>Runs the composed pipeline as if a single request arrived on a connection.</summary>
    /// <param name="terminal">Final handler.</param>
    /// <param name="request">Request payload.</param>
    /// <param name="middlewares">Optional middleware chain.</param>
    /// <returns>Response payload.</returns>
    public static ValueTask<ReadOnlyMemory<byte>> RoundTripAsync(
        TcpConnectionRequestDelegate terminal,
        ReadOnlyMemory<byte> request,
        IEnumerable<ITcpConnectionMiddleware>? middlewares = null)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        var pipeline = TcpConnectionPipeline.Build(terminal, middlewares);
        return pipeline(request);
    }
}
