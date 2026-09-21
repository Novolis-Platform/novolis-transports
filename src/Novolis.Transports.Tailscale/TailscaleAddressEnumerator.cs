using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Novolis.Transports.Tailscale;

/// <summary>Finds IPv4 addresses assigned to an active Tailscale adapter.</summary>
public sealed class TailscaleAddressEnumerator
{
    private const uint TailscaleMinimum = 0x64400000;
    private const uint TailscaleMaximum = 0x647FFFFF;

    /// <summary>Returns unique Tailscale IPv4 addresses.</summary>
    public IReadOnlyList<IPAddress> GetIPv4Addresses()
    {
        var addresses = new List<IPAddress>();
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up
                || !IsTailscaleInterface(networkInterface))
            {
                continue;
            }

            foreach (var address in networkInterface
                         .GetIPProperties()
                         .UnicastAddresses
                         .Select(static item => item.Address)
                         .Where(IsTailscaleIPv4))
            {
                if (!addresses.Contains(address))
                    addresses.Add(address);
            }
        }

        return addresses;
    }

    /// <summary>Returns whether an address belongs to the Tailscale CGNAT range.</summary>
    public static bool IsTailscaleIPv4(IPAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        if (address.AddressFamily != AddressFamily.InterNetwork)
            return false;

        var bytes = address.GetAddressBytes();
        var value = ((uint)bytes[0] << 24)
            | ((uint)bytes[1] << 16)
            | ((uint)bytes[2] << 8)
            | bytes[3];
        return value is >= TailscaleMinimum and <= TailscaleMaximum;
    }

    private static bool IsTailscaleInterface(NetworkInterface networkInterface)
    {
        var name = $"{networkInterface.Name} {networkInterface.Description}";
        return name.Contains("tailscale", StringComparison.OrdinalIgnoreCase);
    }
}
