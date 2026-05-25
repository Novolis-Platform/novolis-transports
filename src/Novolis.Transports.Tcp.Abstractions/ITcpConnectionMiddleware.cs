namespace Novolis.Transports.Tcp.Abstractions;

/// <summary>Middleware invoked around TCP request handling (logging, limits, transforms).</summary>
public interface ITcpConnectionMiddleware
{
    /// <summary>Invokes the middleware, optionally calling <paramref name="next"/>.</summary>
    /// <param name="input">Received payload.</param>
    /// <param name="next">Next delegate in the pipeline.</param>
    /// <returns>Response payload.</returns>
    ValueTask<ReadOnlyMemory<byte>> InvokeAsync(ReadOnlyMemory<byte> input, TcpConnectionRequestDelegate next);
}
