using Novolis.Transports.Torrent.BEncoding;

namespace Novolis.Transports.Unit.Torrent;

public sealed class BEncodedValueRoundTripTests
{
    [Test]
    public async Task Number_encode_decode_roundtrip()
    {
        var original = BEncodedNumber.ToBEncodedNumber(42);
        var decoded = BEncodedValue.Decode(original.Encode());
        await Assert.That(decoded).IsEqualTo(original);
        await Assert.That(((BEncodedNumber)decoded).Number).IsEqualTo(42);
    }

    [Test]
    public async Task String_encode_decode_roundtrip()
    {
        var original = new BEncodedString("announce");
        var decoded = BEncodedValue.Decode(original.Encode());
        await Assert.That(decoded).IsEqualTo(original);
        await Assert.That(((BEncodedString)decoded).Text).IsEqualTo("announce");
    }

    [Test]
    public async Task Dictionary_encode_decode_roundtrip()
    {
        var original = new BEncodedDictionary
        {
            [new BEncodedString("info")] = new BEncodedDictionary
            {
                [new BEncodedString("name")] = new BEncodedString("sample.bin"),
                [new BEncodedString("length")] = BEncodedNumber.ToBEncodedNumber(128)
            }
        };

        var clone = BEncodedValue.Clone(original);
        await Assert.That(clone.Encode()).IsEquivalentTo(original.Encode());
    }
}
