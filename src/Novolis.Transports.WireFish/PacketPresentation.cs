using PacketDotNet;

namespace Novolis.Transports.WireFish;

/// <summary>
/// Builds packet UI strings and detail trees without exposing PacketDotNet in public signatures.
/// </summary>
public static class PacketPresentation
{
    /// <summary>Ethernet / DLT_EN10MB link-layer value (matches PacketDotNet <c>LinkLayers.Ethernet</c>).</summary>
    public const int LinkLayerEthernet = 1;

    /// <summary>Builds a protocol-detail tree from raw capture bytes.</summary>
    /// <param name="raw">Raw frame bytes.</param>
    /// <param name="linkLayer">Link-layer type as an integer (e.g. <see cref="LinkLayerEthernet"/>).</param>
    /// <param name="deviceName">Capture device name (reserved for callers; not embedded in the tree).</param>
    public static PacketDetailNode BuildDetailTree(byte[] raw, int linkLayer, string deviceName)
    {
        ArgumentNullException.ThrowIfNull(raw);
        _ = deviceName;
        var packet = Packet.ParsePacket((LinkLayers)linkLayer, raw);
        return BuildNode(packet);
    }

    /// <summary>Builds a protocol-detail tree from a captured <see cref="DevicePacket"/>.</summary>
    public static PacketDetailNode BuildDetailTree(DevicePacket devicePacket)
    {
        ArgumentNullException.ThrowIfNull(devicePacket);
        var linkLayer = devicePacket.Device is null
            ? LinkLayerEthernet
            : (int)devicePacket.GetDeviceLinkLayerType();
        var deviceName = devicePacket.Device is null ? string.Empty : devicePacket.GetDeviceName();
        return BuildDetailTree(devicePacket.GetRawBytes(), linkLayer, deviceName);
    }

    /// <summary>One-line Info column text for the packet list (TCP/UDP details, ARP/DNS, or truncated summary).</summary>
    public static string FormatInfoLine(DevicePacket devicePacket)
    {
        ArgumentNullException.ThrowIfNull(devicePacket);

        if (devicePacket.IsTcp())
        {
            var tcp = devicePacket.Packet.Extract<TcpPacket>();
            if (tcp is not null)
                return $"TCP {tcp.SourcePort} → {tcp.DestinationPort} [{FormatTcpFlags(tcp)}] Seq={tcp.SequenceNumber} Ack={tcp.AcknowledgmentNumber} Win={tcp.WindowSize}";
        }

        if (devicePacket.IsUdp())
        {
            var udp = devicePacket.Packet.Extract<UdpPacket>();
            if (udp is not null)
                return $"UDP {udp.SourcePort} → {udp.DestinationPort} Len={udp.Length}";
        }

        if (devicePacket.IsArpPacket())
            return "ARP";

        if (devicePacket.IsDnsPacket())
            return "DNS";

        var summary = devicePacket.GetPacketSummary();
        var firstLine = summary.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return firstLine?.Length > 120 ? firstLine[..120] + "…" : firstLine ?? devicePacket.Packet.GetType().Name;
    }

    /// <summary>Display string for the packet protocol column.</summary>
    public static string GetProtocolName(DevicePacket devicePacket)
    {
        ArgumentNullException.ThrowIfNull(devicePacket);
        var ip = devicePacket.Packet.Extract<IPPacket>();
        return ip?.Protocol.ToString() ?? devicePacket.Packet.GetType().Name;
    }

    /// <summary>Source address for list display (IP, else MAC, else "-").</summary>
    public static string GetSourceDisplay(DevicePacket devicePacket)
    {
        ArgumentNullException.ThrowIfNull(devicePacket);
        return devicePacket.GetSourceIPAddress()?.ToString()
            ?? devicePacket.GetMacSourceAddress()?.ToString()
            ?? "-";
    }

    /// <summary>Destination address for list display (IP, else MAC, else "-").</summary>
    public static string GetDestinationDisplay(DevicePacket devicePacket)
    {
        ArgumentNullException.ThrowIfNull(devicePacket);
        return devicePacket.GetDestinationIPAddress()?.ToString()
            ?? devicePacket.GetMacDestinationAddress()?.ToString()
            ?? "-";
    }

    /// <summary>Raw frame bytes from the parsed packet.</summary>
    public static byte[] GetRawBytes(DevicePacket devicePacket)
    {
        ArgumentNullException.ThrowIfNull(devicePacket);
        return devicePacket.Packet.Bytes;
    }

    /// <summary>Link-layer type as an integer suitable for <see cref="BuildDetailTree(byte[], int, string)"/>.</summary>
    public static int GetLinkLayerType(DevicePacket devicePacket)
    {
        ArgumentNullException.ThrowIfNull(devicePacket);
        return devicePacket.Device is null
            ? LinkLayerEthernet
            : (int)devicePacket.GetDeviceLinkLayerType();
    }

    private static PacketDetailNode BuildNode(Packet packet)
    {
        var children = new List<PacketDetailNode>();
        if (packet.PayloadPacket is not null)
            children.Add(BuildNode(packet.PayloadPacket));

        return new PacketDetailNode(
            $"{packet.GetType().Name} ({packet.TotalPacketLength} bytes)",
            Describe(packet),
            children);
    }

    private static string? Describe(Packet packet) => packet switch
    {
        EthernetPacket eth => $"Src={eth.SourceHardwareAddress} Dst={eth.DestinationHardwareAddress} Type={eth.Type}",
        IPv4Packet ip4 => $"Src={ip4.SourceAddress} Dst={ip4.DestinationAddress} TTL={ip4.TimeToLive} Proto={ip4.Protocol}",
        IPv6Packet ip6 => $"Src={ip6.SourceAddress} Dst={ip6.DestinationAddress} Next={ip6.NextHeader}",
        TcpPacket tcp => $"Ports {tcp.SourcePort}→{tcp.DestinationPort} Seq={tcp.SequenceNumber} Ack={tcp.AcknowledgmentNumber}",
        UdpPacket udp => $"Ports {udp.SourcePort}→{udp.DestinationPort} Len={udp.Length}",
        ArpPacket arp => $"WhoHas {arp.TargetProtocolAddress} Tell {arp.SenderProtocolAddress}",
        _ => packet.ToString(StringOutputType.Verbose).Split('\n').FirstOrDefault(),
    };

    private static string FormatTcpFlags(TcpPacket tcp)
    {
        var flags = new List<string>(4);
        if (tcp.Synchronize) flags.Add("SYN");
        if (tcp.Acknowledgment) flags.Add("ACK");
        if (tcp.Finished) flags.Add("FIN");
        if (tcp.Reset) flags.Add("RST");
        if (tcp.Push) flags.Add("PSH");
        return flags.Count == 0 ? "·" : string.Join(",", flags);
    }
}
