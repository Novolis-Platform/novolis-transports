using System.Net;
using FluentAssertions;
using Novolis.Transports.WireFish;
using PacketDotNet;
using PacketDotNet.Utils;
using TUnit.Core;

namespace Novolis.Transports.WireFish.Tests;

public class DevicePacketExtensionsTests
{
    [Test]
    public void GetSourceIPAddress_reads_ipv4_header()
    {
        var bytes = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00,
            0x45, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x40, 0x06, 0x00, 0x00,
            192, 168, 0, 1,
            192, 168, 0, 2,
            0x00, 0x50, 0x00, 0x51, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x50, 0x02, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        var packet = Packet.ParsePacket(LinkLayers.Ethernet, bytes);
        var devicePacket = new DevicePacket(null!, packet, DateTime.UtcNow);

        devicePacket.GetSourceIPAddress().Should().Be(IPAddress.Parse("192.168.0.1"));
        devicePacket.GetDestinationIPAddress().Should().Be(IPAddress.Parse("192.168.0.2"));
        devicePacket.IsTcp().Should().BeTrue();
    }
}
