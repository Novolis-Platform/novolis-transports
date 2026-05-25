namespace Novolis.Transports.Tcp.Abstractions;

/// <summary>Builds a TCP middleware pipeline around a terminal handler.</summary>
public static class TcpConnectionPipeline
{
    /// <summary>Composes middleware in registration order; the terminal handler runs last.</summary>
    /// <param name="terminal">Final handler that produces the response.</param>
    /// <param name="middlewares">Middleware registered outer-first.</param>
    /// <returns>Composed delegate.</returns>
    public static TcpConnectionRequestDelegate Build(
        TcpConnectionRequestDelegate terminal,
        IEnumerable<ITcpConnectionMiddleware>? middlewares)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        var next = terminal;
        if (middlewares is null)
            return next;

        foreach (var middleware in middlewares.Reverse())
        {
            var inner = next;
            next = input => middleware.InvokeAsync(input, inner);
        }

        return next;
    }
}
