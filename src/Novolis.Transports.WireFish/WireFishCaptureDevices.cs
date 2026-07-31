using SharpPcap;
using SharpPcap.LibPcap;

namespace Novolis.Transports.WireFish;

/// <summary>Enumerates live capture devices without exposing SharpPcap to callers.</summary>
public static class WireFishCaptureDevices
{
    /// <summary>
    /// Lists LibPcap live devices, ordered with useful NICs first (Ethernet/Wi‑Fi before WAN miniports).
    /// Returns empty when Npcap/libpcap is unavailable.
    /// </summary>
    public static IReadOnlyList<WireFishCaptureDevice> List()
    {
        try
        {
            return CaptureDeviceList.Instance
                .OfType<LibPcapLiveDevice>()
                .Select(d => new WireFishCaptureDevice(
                    string.IsNullOrWhiteSpace(d.Description) ? d.Name : $"{d.Description} ({d.Name})",
                    d.Name,
                    d.Description ?? string.Empty,
                    d.Interface?.FriendlyName))
                .OrderBy(CaptureDeviceRank.Score)
                .ThenBy(d => d.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>Asks SharpPcap to re-enumerate adapters (call after starting Npcap).</summary>
    public static void Refresh()
    {
        try
        {
            CaptureDeviceList.Instance.Refresh();
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>True when at least one live capture device is available.</summary>
    public static bool Any() => List().Count > 0;
}

/// <summary>Ranks devices so UI defaults land on adapters that usually carry traffic.</summary>
internal static class CaptureDeviceRank
{
    public static int Score(WireFishCaptureDevice device)
    {
        var text = $"{device.Description} {device.FriendlyName} {device.CaptureKey}";
        if (Contains(text, "WAN Miniport")) return 900;
        if (Contains(text, "Loopback") || Contains(text, "NPF_Loopback")) return 800;
        if (Contains(text, "Bluetooth")) return 700;
        if (Contains(text, "Wi-Fi Direct")) return 600;
        if (Contains(text, "Hyper-V") || Contains(text, "vEthernet") || Contains(text, "WSL")) return 500;
        if (Contains(text, "NordLynx") || Contains(text, "OpenVPN") || Contains(text, "TAP-") || Contains(text, "VPN")) return 400;
        if (Contains(text, "Wi-Fi") || Contains(text, "Wireless") || Contains(text, "WLAN")) return 100;
        if (Contains(text, "Ethernet") || Contains(text, "Realtek") || Contains(text, "Intel")) return 50;
        return 200;
    }

    private static bool Contains(string haystack, string needle) =>
        haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
