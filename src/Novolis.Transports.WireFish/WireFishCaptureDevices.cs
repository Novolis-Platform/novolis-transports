using SharpPcap;
using SharpPcap.LibPcap;

namespace Novolis.Transports.WireFish;

/// <summary>Enumerates live capture devices without exposing SharpPcap to callers.</summary>
public static class WireFishCaptureDevices
{
    /// <summary>Lists LibPcap live devices; returns empty when Npcap/libpcap is unavailable.</summary>
    public static IReadOnlyList<WireFishCaptureDevice> List()
    {
        try
        {
            return CaptureDeviceList.Instance
                .OfType<LibPcapLiveDevice>()
                .Select(d => new WireFishCaptureDevice(
                    string.IsNullOrWhiteSpace(d.Description) ? d.Name : $"{d.Description} ({d.Name})",
                    d.Name))
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>True when at least one live capture device is available.</summary>
    public static bool Any() => List().Count > 0;
}
