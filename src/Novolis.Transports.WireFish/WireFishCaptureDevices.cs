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
