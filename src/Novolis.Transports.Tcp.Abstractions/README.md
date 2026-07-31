# Novolis.Transports.Tcp.Abstractions

TCP connection middleware pipeline for request/response handlers. Compose middleware around a terminal delegate; test in-memory with `MemoryTcpTransport`.

## Install

```bash
dotnet add package Novolis.Transports.Tcp.Abstractions
```

## Quick start

```csharp
using Novolis.Transports.Tcp.Abstractions;

ValueTask<ReadOnlyMemory<byte>> Terminal(ReadOnlyMemory<byte> input) =>
    new(Encoding.UTF8.GetBytes("pong"));

var pipeline = TcpConnectionPipeline.Build(Terminal, middlewares:
[
    async (input, next) =>
    {
        var response = await next(input);
        return response; // transform if needed
    },
]);

var response = await MemoryTcpTransport.RoundTripAsync(
    Terminal,
    Encoding.UTF8.GetBytes("ping"),
    middlewares: null);
```

## API

| Type | Role |
|------|------|
| `ITcpConnectionMiddleware` | `InvokeAsync(input, next)` |
| `TcpConnectionRequestDelegate` | Terminal handler signature |
| `TcpConnectionPipeline.Build` | Compose middleware + terminal |
| `MemoryTcpTransport.RoundTripAsync` | In-memory round-trip for tests |

## Related

| Package | Role |
|---------|------|
| `Novolis.Transports.Tcp.Client` | TCP client send |
| `Novolis.Transports.Tcp.Server` | Kestrel-hosted TCP listener |
| `Novolis.Transports.Tcp.Cryptography` | Optional AES payload encryption (internal package) |
