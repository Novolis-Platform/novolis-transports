# Novolis.Transports.Tcp.Abstractions

TCP connection middleware pipeline contracts and in-memory round-trip testing (`MemoryTcpTransport`).

## Install

```bash
dotnet add package Novolis.Transports.Tcp.Abstractions
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Transports.Tcp.Abstractions;

static ValueTask<ReadOnlyMemory<byte>> EchoHandler(ReadOnlyMemory<byte> request) =>
    ValueTask.FromResult(request);

var response = await MemoryTcpTransport.RoundTripAsync(
    EchoHandler,
    new byte[] { 1, 2, 3 });
```

Compose middleware with `TcpConnectionPipeline.Build` before the terminal handler.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Transports.Tcp.Client` | DI TCP client |
| `Novolis.Transports.Tcp.Server` | Kestrel TCP server hosting |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/getting-started.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages).
