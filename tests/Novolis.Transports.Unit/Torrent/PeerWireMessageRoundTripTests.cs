using Novolis.Transports.Torrent.PeerWireProtocol.Messages;

namespace Novolis.Transports.Unit.Torrent;

public sealed class PeerWireMessageRoundTripTests
{
    [Test]
    public async Task KeepAliveMessage_encode_decode_roundtrip()
    {
        var message = new KeepAliveMessage();
        var buffer = new byte[message.Length];
        message.Encode(buffer, 0);

        var offset = 0;
        var decoded = KeepAliveMessage.TryDecode(buffer, ref offset, out var roundTrip);
        await Assert.That(decoded).IsTrue();
        await Assert.That(roundTrip).IsNotNull();
    }
}
