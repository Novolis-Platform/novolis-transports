using Novolis.Transports.Datagrams;

namespace Novolis.Transports.Unit.Datagrams;

public sealed class DatagramReassemblerTests
{
    [Test]
    public async Task FragmentsReassembleOutOfOrder()
    {
        var payload = Enumerable.Range(0, 40).Select(static i => (byte)i).ToArray();
        var fragments = Split(sequence: 9, payload, fragmentSize: 16);
        var reassembler = new DatagramReassembler();
        byte[]? result = null;
        var now = DateTimeOffset.UtcNow;

        foreach (var fragment in fragments.Reverse())
        {
            if (reassembler.TryAccept(fragment, now, out var completed))
                result = completed;
        }

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).IsEquivalentTo(payload);
    }

    [Test]
    public async Task IncompleteSequenceExpiresAndDoesNotBlockLaterFrame()
    {
        var reassembler = new DatagramReassembler(
            maximumAge: TimeSpan.FromMilliseconds(10));
        var first = Split(sequence: 1, [1, 2, 3, 4], fragmentSize: 2);
        var secondPayload = "next frame"u8.ToArray();
        var second = Split(sequence: 2, secondPayload, fragmentSize: 32);
        var now = DateTimeOffset.UtcNow;

        reassembler.TryAccept(first[0], now, out _);
        var completed = reassembler.TryAccept(
            second[0],
            now.AddMilliseconds(20),
            out var result);

        await Assert.That(completed).IsTrue();
        await Assert.That(result).IsEquivalentTo(secondPayload);
        await Assert.That(reassembler.PendingCount).IsEqualTo(0);
        await Assert.That(
                reassembler.TryAccept(first[0], now.AddMilliseconds(30), out _))
            .IsFalse();
    }

    private static DatagramFragment[] Split(
        long sequence,
        byte[] payload,
        int fragmentSize)
    {
        var count = (ushort)Math.Ceiling(payload.Length / (double)fragmentSize);
        var fragments = new DatagramFragment[count];
        for (ushort index = 0; index < count; index++)
        {
            var offset = index * fragmentSize;
            var length = Math.Min(fragmentSize, payload.Length - offset);
            fragments[index] = new DatagramFragment(
                sequence,
                index,
                count,
                payload.AsSpan(offset, length).ToArray());
        }

        return fragments;
    }
}
