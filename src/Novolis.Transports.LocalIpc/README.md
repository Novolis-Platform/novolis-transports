# Novolis.Transports.LocalIpc

Local IPC transport for typed request/response and event-stream messaging over named pipes or Unix domain sockets.

## Install

```bash
dotnet add package Novolis.Transports.LocalIpc
```

## Quick start

```csharp
using Novolis.Transports.LocalIpc;

var endpoint = new LocalIpcEndpoint("novolis-audio-live");
using var client = LocalIpcTransport.CreateClient();
await using var connection = await client.ConnectAsync(endpoint);
```
