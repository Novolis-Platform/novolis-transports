namespace Novolis.Transports;

/// <summary>Snapshot of channel activity counters.</summary>
public readonly record struct TransportChannelStatistics(
    long SentPackets,
    long SentBytes,
    long ReceivedPackets,
    long ReceivedBytes,
    long DroppedPackets,
    long RejectedPackets);
