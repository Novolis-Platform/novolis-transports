using System.Net;
using System.Net.NetworkInformation;

namespace Novolis.Transports.NetworkPath;

/// <summary>Portable information collected from one local network adapter.</summary>
public sealed record NetworkAdapterInfo(
    string Id,
    string Name,
    string Description,
    NetworkInterfaceType InterfaceType,
    OperationalStatus Status,
    int? InterfaceIndex,
    IReadOnlyList<IPAddress> Addresses,
    IReadOnlyList<IPAddress> Gateways)
{
    public bool IsOperational => this.Status == OperationalStatus.Up;

    public bool HasDefaultGateway => this.Gateways.Count > 0;

    public bool IsTunnelCandidate =>
        this.InterfaceType == NetworkInterfaceType.Tunnel ||
        ContainsTunnelHint(this.Name) ||
        ContainsTunnelHint(this.Description);

    public IReadOnlyList<string> TunnelEvidence
    {
        get
        {
            var evidence = new List<string>();
            if (this.InterfaceType == NetworkInterfaceType.Tunnel)
                evidence.Add("network interface type is Tunnel");
            if (ContainsTunnelHint(this.Name))
                evidence.Add($"adapter name contains a tunnel hint: {this.Name}");
            if (ContainsTunnelHint(this.Description) &&
                !string.Equals(this.Description, this.Name, StringComparison.OrdinalIgnoreCase))
            {
                evidence.Add($"adapter description contains a tunnel hint: {this.Description}");
            }

            return evidence;
        }
    }

    private static bool ContainsTunnelHint(string value)
    {
        foreach (var hint in new[] { "tun", "tap", "vpn", "wireguard", "tailscale", "zerotier", "openvpn" })
        {
            if (value.Contains(hint, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
