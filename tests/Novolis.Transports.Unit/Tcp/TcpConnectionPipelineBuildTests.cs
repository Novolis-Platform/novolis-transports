using Novolis.Transports.Tcp.Abstractions;

namespace Novolis.Transports.Unit.Tcp;

public sealed class TcpConnectionPipelineBuildTests
{
    [Test]
    public async Task Build_with_null_middleware_returns_terminal()
    {
        var called = false;
        TcpConnectionRequestDelegate terminal = input =>
        {
            called = true;
            return ValueTask.FromResult(input);
        };

        var pipeline = TcpConnectionPipeline.Build(terminal, null);
        var result = await pipeline(new byte[] { 1, 2, 3 });
        await Assert.That(called).IsTrue();
        await Assert.That(result.ToArray()).IsEquivalentTo(new byte[] { 1, 2, 3 });
    }

    [Test]
    public async Task Build_with_empty_middleware_returns_terminal()
    {
        TcpConnectionRequestDelegate terminal = input => ValueTask.FromResult(input);
        var pipeline = TcpConnectionPipeline.Build(
            terminal,
            Array.Empty<ITcpConnectionMiddleware>());
        var result = await pipeline(new byte[] { 7 });
        await Assert.That(result.ToArray()).IsEquivalentTo(new byte[] { 7 });
    }
}
