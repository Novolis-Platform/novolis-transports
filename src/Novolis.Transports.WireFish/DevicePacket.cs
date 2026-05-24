using PacketDotNet;
using SharpPcap.LibPcap;

namespace Novolis.Transports.WireFish;

/// <summary>Captured packet with device and timestamp metadata.</summary>
/// <param name="Device">LibPcap device that captured the packet.</param>
/// <param name="Packet">Parsed packet.</param>
/// <param name="Timestamp">Capture timestamp.</param>
public record DevicePacket(LibPcapLiveDevice Device, Packet Packet, DateTime Timestamp);
