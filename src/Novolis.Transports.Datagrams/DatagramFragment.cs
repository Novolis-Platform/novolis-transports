namespace Novolis.Transports.Datagrams;

/// <summary>One ordered fragment of a datagram payload.</summary>
public readonly record struct DatagramFragment(
    long Sequence,
    ushort FragmentIndex,
    ushort FragmentCount,
    byte[] Payload);
