# Novolis.Transports.LocalIpc

Framed local IPC transport for typed request/response and event-stream messaging over **named pipes** (Windows) or **Unix domain sockets** (non-Windows). Domain-agnostic — typed RPC helpers live in consumer packages (`Novolis.Agent.Surface`, `Novolis.Avalonia.Agent.Protocol`, etc.).

## Install

```bash
dotnet add package Novolis.Transports.LocalIpc
```

## Quick start — client

```csharp
using Novolis.Transports.LocalIpc;

var endpoint = new LocalIpcEndpoint("novolis-audio-live");
using var client = LocalIpcTransport.CreateClient();
await using var connection = await client.ConnectAsync(endpoint);

await connection.SendAsync(new LocalIpcFrame(
    Sequence: 1,
    Kind: "request",
    Name: "Snapshot",
    Payload: Array.Empty<byte>()));

await foreach (var frame in connection.ReadAllAsync())
{
    // handle response/event frames
}
```

## Quick start — server

```csharp
var endpoint = new LocalIpcEndpoint("novolis-audio-live");
await using var listener = LocalIpcTransport.CreateListener(endpoint);

while (true)
{
    await using var connection = await listener.AcceptAsync();
    await foreach (var frame in connection.ReadAllAsync())
    {
        await connection.SendAsync(new LocalIpcFrame(
            frame.Sequence, "response", frame.Name, responseBytes));
    }
}
```

## Frame protocol

Each frame on the wire: **4-byte little-endian length** + binary body:

- `long` sequence
- UTF-8 `kind` (e.g. `request`, `response`, `event`)
- UTF-8 `name` (operation or event name)
- `byte[]` payload

`LocalIpcFrameCodec.WriteAsync` / `ReadAsync` handle encoding. `ReadAsync` returns `null` on clean EOF.

`LocalIpcTransportKind.Auto` picks named pipe on Windows, Unix socket elsewhere.

## API

| Type | Role |
|------|------|
| `LocalIpcTransport` | `CreateClient()`, `CreateListener(endpoint)` |
| `LocalIpcEndpoint` | `(Address, Kind)` — pipe name or socket path |
| `LocalIpcTransportKind` | `Auto`, `NamedPipe`, `UnixDomainSocket` |
| `LocalIpcFrame` | `(Sequence, Kind, Name, Payload)` |
| `LocalIpcFrameCodec` | Stream read/write helpers |
| `ILocalIpcClient` | `ConnectAsync` → `ILocalIpcConnection` |
| `ILocalIpcConnection` | `SendAsync`, `ReadAllAsync`, `IAsyncDisposable` |
| `ILocalIpcListener` | `AcceptAsync`, `IAsyncDisposable` |

## Dogfooding / apps

Used by Live Studio audio host, Novolis Agent surface, Avalonia Agent MCP UI, and AvaloniaAgentMcp session runtime.

## Related

| Package | Role |
|---------|------|
| `Novolis.Agent.Surface` | Agent host IPC + typed message helpers |
| `Novolis.Avalonia.Agent.Protocol` | UI agent client over default endpoint |
| `Novolis.Transports.Http` | Remote HTTP transport (complementary) |
