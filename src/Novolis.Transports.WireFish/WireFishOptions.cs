namespace Novolis.Transports.WireFish;

/// <summary>Capture settings for WireFish hosted services.</summary>
public class WireFishOptions
{
    /// <summary>When true, opens every available capture device. When false, only <see cref="DeviceNames"/> are used.</summary>
    public bool CaptureAllDevices { get; set; } = true;

    /// <summary>Device names or friendly names to capture when <see cref="CaptureAllDevices"/> is false.</summary>
    public IList<string> DeviceNames { get; } = [];

    /// <summary>Optional Berkeley Packet Filter expression applied to each opened device.</summary>
    public string? BpfFilter { get; set; }

    /// <summary>Whether adapters are opened in promiscuous mode.</summary>
    public bool PromiscuousMode { get; set; } = true;

    /// <summary>When no capture devices are available, log a warning and skip capture instead of failing startup.</summary>
    public bool AllowNoCaptureDevices { get; set; } = true;
}
