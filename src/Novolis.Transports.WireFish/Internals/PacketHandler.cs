using System.Threading.Channels;
using PacketDotNet;
using SharpPcap.LibPcap;

namespace Novolis.Transports.WireFish.Internals;

internal class PacketHandler(ChannelWriter<DevicePacket> writer)
{
    public ValueTask EnqueueAsync(LibPcapLiveDevice device, Packet packet, CancellationToken cancellationToken = default)
    {
        var devicePacket = new DevicePacket(device, packet, DateTime.UtcNow);
        return writer.WriteAsync(devicePacket, cancellationToken);
    }
}
