namespace Novolis.Transports.WireFish;

/// <summary>Capture NIC available for WireFish live capture (no SharpPcap types).</summary>
/// <param name="DisplayName">Human-readable label for UI lists.</param>
/// <param name="CaptureKey">Value for <see cref="WireFishOptions.DeviceNames"/> (device name).</param>
public sealed record WireFishCaptureDevice(string DisplayName, string CaptureKey)
{
    /// <inheritdoc />
    public override string ToString() => DisplayName;
}
