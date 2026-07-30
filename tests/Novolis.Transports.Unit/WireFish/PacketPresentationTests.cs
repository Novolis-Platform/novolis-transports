using Novolis.Transports.WireFish;
using PacketDotNet;
using TUnit.Core;

namespace Novolis.Transports.WireFish.Tests;

public class PacketPresentationTests
{
    private static readonly byte[] TcpOverIpv4 =
    [
        0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x08, 0x00,
        0x45, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x40, 0x06, 0x00, 0x00,
        192, 168, 1, 1,
        192, 168, 1, 2,
        0x04, 0xD2, 0x01, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x50, 0x02, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00
    ];

    [Test]
    public async Task BuildDetailTree_tcpOverIpv4_has_ethernet_ip_tcp_layers()
    {
        var root = PacketPresentation.BuildDetailTree(TcpOverIpv4, PacketPresentation.LinkLayerEthernet, "test0");
        await Assert.That(root.Title).Contains("EthernetPacket");
        await Assert.That(root.Children.Count).IsEqualTo(1);
        await Assert.That(root.Children[0].Title).Contains("IPv4Packet");
        await Assert.That(root.Children[0].Children[0].Title).Contains("TcpPacket");
    }

    [Test]
    public async Task FormatInfoLine_tcp_includes_ports_and_flags()
    {
        var packet = Packet.ParsePacket(LinkLayers.Ethernet, TcpOverIpv4);
        var devicePacket = new DevicePacket(null!, packet, DateTime.UtcNow);

        var info = PacketPresentation.FormatInfoLine(devicePacket);
        await Assert.That(info).Contains("TCP");
        await Assert.That(info).Contains("1234");
        await Assert.That(info).Contains("443");
        await Assert.That(info).Contains("SYN");
    }

    [Test]
    public async Task GetProtocolName_returns_Tcp()
    {
        var packet = Packet.ParsePacket(LinkLayers.Ethernet, TcpOverIpv4);
        var devicePacket = new DevicePacket(null!, packet, DateTime.UtcNow);

        await Assert.That(PacketPresentation.GetProtocolName(devicePacket)).IsEqualTo("Tcp");
        await Assert.That(PacketPresentation.GetSourceDisplay(devicePacket)).IsEqualTo("192.168.1.1");
        await Assert.That(PacketPresentation.GetDestinationDisplay(devicePacket)).IsEqualTo("192.168.1.2");
    }
}
