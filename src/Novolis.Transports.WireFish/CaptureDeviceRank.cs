using SharpPcap;
using SharpPcap.LibPcap;

namespace Novolis.Transports.WireFish;

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
