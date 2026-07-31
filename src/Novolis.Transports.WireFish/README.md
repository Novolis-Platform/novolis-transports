# Novolis.Transports.WireFish

Live packet capture (SharpPcap) with `Novolis.Messaging.Channels` and hosted `IPacketHandler` dispatch. Migrated from `Frank.WireFish` / `novolis-wirefish`.

## Install

```bash
dotnet add package Novolis.Transports.WireFish
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`), Npcap (Windows) or libpcap. Elevation may be required to start the Npcap service.

## Quick start

```csharp
using Novolis.Transports.WireFish;

builder.Services.AddNovolisWireFish(
    w => w.AddPacketHandler<MyPacketHandler>(),
    o =>
    {
        o.BpfFilter = "tcp port 443";
        o.CaptureAllDevices = false;
        o.DeviceNames = ["\\Device\\NPF_{...}"];
    });
```

Handler template:

```csharp
public sealed class MyPacketHandler : IPacketHandler
{
    public bool CanHandle(DevicePacket packet) => packet.IsTcp();
    public Task HandleAsync(DevicePacket packet, CancellationToken cancellationToken) => Task.CompletedTask;
}
```

Device discovery and startup health:

```csharp
var devices = WireFishCaptureDevices.List();
var health = WireFishCaptureHealthChecks.Check();
WireFishCaptureHealthChecks.TryEnsureCaptureDriver();
```

Presentation helpers (no PacketDotNet in public signatures): `DevicePacketExtensions`, `PacketPresentation`.

## API

| Type | Role |
|------|------|
| `ServiceCollectionExtensions.AddNovolisWireFish` | Register handlers + hosted capture |
| `IWireFishBuilder` | `AddPacketHandler<THandler>()` |
| `WireFishOptions` | `CaptureAllDevices`, `DeviceNames`, `BpfFilter`, `PromiscuousMode`, `AllowNoCaptureDevices` |
| `DevicePacket` | `(Device, Packet, Timestamp)` capture unit |
| `IPacketHandler` | `CanHandle`, `HandleAsync` |
| `WireFishCaptureDevices` | `List()`, `Refresh()`, `Any()` |
| `WireFishCaptureHealthChecks` | Driver/service readiness checks |
| `WireFishCaptureHealth` | `(IsReady, Message)` |
| `PacketPresentation` / `PacketDetailNode` | UI-friendly formatting and protocol tree |

Obsolete `Frank.WireFish.*` type aliases ship for one preview cycle.

## Dogfooding / apps

Used by **WireFishViewer** (`novolis-dogfooding`) for live capture UI.

## Related

| Package | Role |
|---------|------|
| `Novolis.Messaging.Channels` | `Channel<DevicePacket>` pipeline |
| `novolis-wirefish` | Redirect repo — use this package instead |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/getting-started.md)

## Support

Pre-release. Legacy `Frank.WireFish` type aliases are obsolete.
