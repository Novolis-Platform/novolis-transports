using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Novolis.Transports.WireFish;

/// <summary>Driver / service readiness for live capture (no SharpPcap types).</summary>
/// <param name="IsReady">True when capture devices can be enumerated and the host driver looks usable.</param>
/// <param name="Message">Human-readable guidance when <see cref="IsReady"/> is false; otherwise null.</param>
public sealed record WireFishCaptureHealth(bool IsReady, string? Message);

/// <summary>Checks Npcap/libpcap readiness for the WireFish viewer and hosts.</summary>
public static class WireFishCaptureHealthChecks
{
    /// <summary>Probes the capture driver and device list.</summary>
    public static WireFishCaptureHealth Check()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var npcap = QueryNpcapService();
            if (npcap is NpcapServiceState.Missing)
            {
                return new WireFishCaptureHealth(
                    false,
                    "Npcap is not installed. Install from https://npcap.com/ and restart this app.");
            }

            if (npcap is NpcapServiceState.Stopped)
            {
                return new WireFishCaptureHealth(
                    false,
                    "Npcap service is stopped, so adapters cannot be opened. Accept the UAC prompt on launch (or run elevated), or: Start-Service npcap");
            }
        }

        var devices = WireFishCaptureDevices.List();
        if (devices.Count == 0)
        {
            return new WireFishCaptureHealth(
                false,
                "No capture devices found. Install Npcap (Windows) or libpcap, then restart.");
        }

        return new WireFishCaptureHealth(true, null);
    }

    /// <summary>
    /// On Windows, when the process is elevated and Npcap is installed but stopped, starts the service
    /// and refreshes the capture device list. No-op on other OS or when already running.
    /// </summary>
    /// <returns>True when Npcap is running afterward (or non-Windows); false if start failed.</returns>
    public static bool TryEnsureCaptureDriver() => TryStartNpcap(allowElevationPrompt: false).IsReady;

    /// <summary>
    /// Attempts to start the Npcap service and refresh capture devices.
    /// When <paramref name="allowElevationPrompt"/> is true and this process is not elevated,
    /// prompts UAC to run <c>sc start npcap</c>.
    /// </summary>
    public static WireFishCaptureHealth TryStartNpcap(bool allowElevationPrompt = true)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WireFishCaptureHealth(true, null);

        var state = QueryNpcapService();
        if (state is NpcapServiceState.Running)
        {
            WireFishCaptureDevices.Refresh();
            return new WireFishCaptureHealth(true, null);
        }

        if (state is NpcapServiceState.Missing)
        {
            return new WireFishCaptureHealth(
                false,
                "Npcap is not installed. Install from https://npcap.com/ and restart this app.");
        }

        (bool ok, string? detail) start = IsProcessElevated()
            ? TryStartNpcapServiceDetailed()
            : allowElevationPrompt
                ? TryStartNpcapServiceElevatedDetailed()
                : (false, "Elevation required to start Npcap.");

        if (!start.ok)
        {
            return new WireFishCaptureHealth(
                false,
                FormatStartFailure(start.detail));
        }

        // Elevated sc.exe can return before the service reports RUNNING.
        for (var i = 0; i < 20; i++)
        {
            if (QueryNpcapService() is NpcapServiceState.Running)
                break;
            Thread.Sleep(100);
        }

        WireFishCaptureDevices.Refresh();
        if (QueryNpcapService() is NpcapServiceState.Running)
            return new WireFishCaptureHealth(true, null);

        var win32 = QueryNpcapWin32ExitCode();
        return new WireFishCaptureHealth(
            false,
            FormatDriverLoadFailure(win32));
    }

    private static string FormatStartFailure(string? detail)
    {
        if (!IsProcessElevated() && string.IsNullOrWhiteSpace(detail))
        {
            return "Npcap is stopped and elevation was cancelled or failed. Accept the UAC prompt, or run: Start-Service npcap";
        }

        var win32 = QueryNpcapWin32ExitCode();
        if (win32 is 31 or 2 or 577 or 1275)
            return FormatDriverLoadFailure(win32);

        return string.IsNullOrWhiteSpace(detail)
            ? "Failed to start the Npcap service."
            : $"Failed to start Npcap: {detail}";
    }

    private static string FormatDriverLoadFailure(int? win32ExitCode)
    {
        var code = win32ExitCode is null ? "" : $" (Win32 {win32ExitCode})";
        return
            $"Npcap's kernel driver failed to load{code} — this is an OS/driver problem, not WireFish. " +
            "Wireshark/dumpcap will be broken the same way. Repair: run elevated " +
            @"'C:\Program Files\Npcap\FixInstall.bat', or reinstall Npcap from https://npcap.com/ and reboot.";
    }

    /// <summary>True when the current process token is in the Administrators role.</summary>
    public static bool IsProcessElevated()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return false;

        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>True when Npcap is installed on Windows and currently stopped (button affordance).</summary>
    public static bool IsNpcapStopped()
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
           && QueryNpcapService() is NpcapServiceState.Stopped;

    private enum NpcapServiceState
    {
        Running,
        Stopped,
        Missing,
        Unknown,
    }

    private static (bool Ok, string? Detail) TryStartNpcapServiceElevatedDetailed()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = "start npcap",
                UseShellExecute = true,
                Verb = "runas",
            });
            if (process is null)
                return (false, "Could not launch elevated sc.exe.");

            if (!process.WaitForExit(15_000))
            {
                try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
                return (false, "Elevated sc.exe timed out.");
            }

            if (process.ExitCode is 0 or 1056 || QueryNpcapService() is NpcapServiceState.Running)
                return (true, null);

            return (false, $"sc.exe exit {process.ExitCode}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static (bool Ok, string? Detail) TryStartNpcapServiceDetailed()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = "start npcap",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            if (process is null)
                return (false, "Could not launch sc.exe.");

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(15_000))
            {
                try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
                return (false, "sc.exe timed out.");
            }

            if (process.ExitCode is 0 or 1056 || QueryNpcapService() is NpcapServiceState.Running)
                return (true, null);

            var detail = string.Join(" ", new[] { stderr, stdout }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
            if (string.IsNullOrWhiteSpace(detail))
                detail = $"sc.exe exit {process.ExitCode}";
            return (false, detail);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static int? QueryNpcapWin32ExitCode()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = "query npcap",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            if (process is null)
                return null;

            var output = process.StandardOutput.ReadToEnd();
            if (!process.WaitForExit(3000))
            {
                try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
                return null;
            }

            // WIN32_EXIT_CODE    : 31  (0x1f)
            foreach (var line in output.Split('\n'))
            {
                if (!line.Contains("WIN32_EXIT_CODE", StringComparison.OrdinalIgnoreCase))
                    continue;
                var parts = line.Split(':', 2);
                if (parts.Length < 2)
                    continue;
                var token = parts[1].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (int.TryParse(token, out var code))
                    return code;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static NpcapServiceState QueryNpcapService()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = "query npcap",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            if (process is null)
                return NpcapServiceState.Unknown;

            var output = process.StandardOutput.ReadToEnd();
            if (!process.WaitForExit(3000))
            {
                try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
                return NpcapServiceState.Unknown;
            }

            if (output.Contains("FAILED", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("1060", StringComparison.Ordinal))
                return NpcapServiceState.Missing;

            if (output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase))
                return NpcapServiceState.Running;

            if (output.Contains("STOPPED", StringComparison.OrdinalIgnoreCase))
                return NpcapServiceState.Stopped;

            return NpcapServiceState.Unknown;
        }
        catch
        {
            return NpcapServiceState.Unknown;
        }
    }
}
