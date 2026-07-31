# Novolis.Transports.Tcp.Server

Kestrel-hosted TCP server: listen on a port and dispatch connections to `IConnectionHandler` implementations.

## Install

```bash
dotnet add package Novolis.Transports.Tcp.Server
```

## Quick start — minimal host

```csharp
using Novolis.Transports.Tcp.Server;

public sealed class EchoHandler : IConnectionHandler
{
    public Task<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> request) =>
        Task.FromResult(request);
}

await Server.CreateTcpServer<EchoHandler>(port: 9000).RunAsync();
```

## Quick start — ASP.NET Core

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.UseTcpConnectionHandler<EchoHandler>(port: 9000);
```

Also available on `IWebHostBuilder` and `IHostBuilder` via `UseTcpConnectionHandler<THandler>(port)`.

## API

| Type | Role |
|------|------|
| `IConnectionHandler` | `HandleAsync(ReadOnlyMemory<byte>)` |
| `Server.CreateTcpServer<THandler>` | Standalone TCP host |
| `WebApplicationBuilderExtensions.UseTcpConnectionHandler` | ASP.NET Core integration |
| `WebHostBuilderExtensions.UseTcpConnectionHandler` | Generic host integration |
| `TcpServerHostBuilderExtensions.UseTcpConnectionHandler` | `IHostBuilder` integration |

## Related

| Package | Role |
|---------|------|
| `Novolis.Transports.Tcp.Client` | Send requests to this server |
| `Novolis.Transports.Tcp.Abstractions` | Middleware pipeline |
| `Novolis.Transports.Tcp.Cryptography` | Payload encryption (used by client) |
