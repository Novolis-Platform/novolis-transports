using Novolis.Transports.LocalIpc;

namespace Novolis.Transports.Unit.LocalIpc;

public sealed class LocalIpcFrameCodecTests
{
    [Test]
    public async Task Round_trips_frame_payload()
    {
        await using var stream = new MemoryStream();
        var frame = new LocalIpcFrame(42, "request", "compile", [1, 2, 3, 4]);

        await LocalIpcFrameCodec.WriteAsync(stream, frame);
        stream.Position = 0;

        var read = await LocalIpcFrameCodec.ReadAsync(stream);

        await Assert.That(read).IsNotNull();
        await Assert.That(read!.Sequence).IsEqualTo(42);
        await Assert.That(read.Kind).IsEqualTo("request");
        await Assert.That(read.Name).IsEqualTo("compile");
        await Assert.That(read.Payload).IsEquivalentTo(new byte[] { 1, 2, 3, 4 });
    }
}
