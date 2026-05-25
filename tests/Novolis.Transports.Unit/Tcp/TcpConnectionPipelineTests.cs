using Novolis.Transports.Tcp.Abstractions;
using TUnit.Core;

namespace Novolis.Transports.Tcp.Tests;

public sealed class TcpConnectionPipelineTests
{
    [Test]
    public async Task MemoryTcpTransport_RoundTripsThroughMiddleware()
    {
        var observed = new List<string>();
        ITcpConnectionMiddleware outer = new RecordingMiddleware("outer", observed);
        ITcpConnectionMiddleware inner = new RecordingMiddleware("inner", observed);

        TcpConnectionRequestDelegate terminal = input =>
        {
            observed.Add("terminal");
            return ValueTask.FromResult(input);
        };

        var response = await MemoryTcpTransport.RoundTripAsync(
            terminal,
            new byte[] { 9, 8, 7 },
            [outer, inner]);

        await Assert.That(response.ToArray()).IsEqualTo(new byte[] { 9, 8, 7 });
        await Assert.That(observed).IsEqualTo(new[] { "outer-in", "inner-in", "terminal", "inner-out", "outer-out" });
    }

    private sealed class RecordingMiddleware(string name, List<string> trace) : ITcpConnectionMiddleware
    {
        public async ValueTask<ReadOnlyMemory<byte>> InvokeAsync(
            ReadOnlyMemory<byte> input,
            TcpConnectionRequestDelegate next)
        {
            trace.Add($"{name}-in");
            var response = await next(input);
            trace.Add($"{name}-out");
            return response;
        }
    }
}
