# Novolis.Transports.WireFish

Live packet capture (SharpPcap) with `Novolis.Messaging.Channels` and hosted handlers.

## Install

```bash
dotnet add package Novolis.Transports.WireFish
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`), Npcap/WinPcap or compatible capture driver where required.

## Quick start

```csharp
services.AddNovolisWireFish(
    w => w.AddPacketHandler<MyPacketHandler>(),
    o => o.BpfFilter = "tcp port 443");
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Messaging.Channels` | Channel pipeline for `DevicePacket` |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/getting-started.md)

## Support

Pre-release. Legacy `Frank.WireFish` type aliases are obsolete.
