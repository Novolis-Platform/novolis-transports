namespace Novolis.Transports.WireFish;

/// <summary>Capture NIC available for WireFish live capture (no SharpPcap types).</summary>
/// <param name="DisplayName">Human-readable label for UI lists.</param>
/// <param name="CaptureKey">Value for <see cref="WireFishOptions.DeviceNames"/> (device name).</param>
/// <param name="Description">Raw adapter description from the capture driver.</param>
/// <param name="FriendlyName">OS friendly name when available (e.g. "Ethernet").</param>
public sealed record WireFishCaptureDevice(
    string DisplayName,
    string CaptureKey,
    string Description = "",
    string? FriendlyName = null)
{
    /// <inheritdoc />
    public override string ToString() => DisplayName;
}
