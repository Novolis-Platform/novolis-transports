using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Novolis.Transports.WireFish;

/// <summary>Driver / service readiness for live capture (no SharpPcap types).</summary>
/// <param name="IsReady">True when capture devices can be enumerated and the host driver looks usable.</param>
/// <param name="Message">Human-readable guidance when <see cref="IsReady"/> is false; otherwise null.</param>
public sealed record WireFishCaptureHealth(bool IsReady, string? Message);
