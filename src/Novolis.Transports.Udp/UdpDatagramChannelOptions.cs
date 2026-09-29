namespace Novolis.Transports.Udp;

/// <summary>Configuration for a bounded UDP datagram channel.</summary>
public sealed record UdpDatagramChannelOptions
{
    /// <summary>Conservative payload ceiling that avoids IP fragmentation.</summary>
    public int MaximumPayloadSize { get; init; } = 1_200;

    /// <summary>Maximum number of received datagrams waiting for a consumer.</summary>
    public int ReceiveQueueCapacity { get; init; } = 128;

    /// <summary>Enables broadcast sends on the underlying socket.</summary>
    public bool EnableBroadcast { get; init; }
}
