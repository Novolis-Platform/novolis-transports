using PacketDotNet;
using SharpPcap.LibPcap;

namespace Novolis.Transports.WireFish;

/// <summary>
/// Represents a device packet containing information about the captured packet.
/// </summary>
public record DevicePacket(LibPcapLiveDevice Device, Packet Packet, DateTime Timestamp);