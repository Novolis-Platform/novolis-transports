# Novolis.Transports.Tcp.Client

Simple DI-registered TCP client with configurable read timeout.

## Install

```bash
dotnet add package Novolis.Transports.Tcp.Client
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
services.AddTcpClient(o => o.Timeout = TimeSpan.FromSeconds(10));
var response = await client.SendAsync(ip, port, payload);
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Transports.Tcp.Cryptography` | AES payload encryption helpers |
| `Novolis.Transports.Tcp.Server` | Kestrel TCP server hosting |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/getting-started.md)

## Support

Pre-release.
