using Novolis.Transports.Torrent.TrackerProtocol.Udp.Messages;

namespace Novolis.Transports.Unit.Torrent;

public sealed class UdpConnectMessageRoundTripTests
{
    [Test]
    public async Task ConnectMessage_encode_decode_roundtrip()
    {
        var message = new ConnectMessage(12345);
        var buffer = new byte[message.Length];
        message.Encode(buffer, 0);

        var decoded = ConnectMessage.TryDecode(buffer, 0, out var roundTrip);
        await Assert.That(decoded).IsTrue();
        await Assert.That(roundTrip!.TransactionId).IsEqualTo(12345);
    }
}
