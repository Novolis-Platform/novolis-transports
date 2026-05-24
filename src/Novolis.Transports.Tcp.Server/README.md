# Novolis.Transports.Tcp.Server

Kestrel-based TCP server hosting with per-connection `IConnectionHandler` dispatch.

## Install

```bash
dotnet add package Novolis.Transports.Tcp.Server
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
var host = Server.CreateTcpServer<MyHandler>(port: 9000);
await host.RunAsync();
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Transports.Tcp.Client` | TCP client |
| `Novolis.Transports.Tcp.Cryptography` | Payload encryption |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/getting-started.md)

## Support

Pre-release.
