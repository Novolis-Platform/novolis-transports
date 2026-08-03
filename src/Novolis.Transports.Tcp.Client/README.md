<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-transports">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Transports.Tcp.Client

TCP client for sending byte payloads to a remote host. Automatically registers TCP payload encryption helpers.

## Install

```bash
dotnet add package Novolis.Transports.Tcp.Client
```

Depends on `Novolis.Transports.Tcp.Abstractions` and `Novolis.Transports.Tcp.Cryptography`.

## Quick start

```csharp
using Microsoft.Extensions.DependencyInjection;
using Novolis.Transports.Tcp.Client;

services.AddTcpClient(o => o.SendTimeout = TimeSpan.FromSeconds(5));

var client = sp.GetRequiredService<ITcpClient>();
var response = await client.SendAsync(
    IPAddress.Loopback,
    port: 9000,
    data: Encoding.UTF8.GetBytes("ping"));
```

## API

| Type | Role |
|------|------|
| `ITcpClient` | `SendAsync(IPAddress, port, data)` |
| `TcpClient` | Default implementation |
| `TcpClientOptions` | Timeout and client options |
| `ServiceCollectionExtensions.AddTcpClient` | Registers client + encryption |

## Related

| Package | Role |
|---------|------|
| `Novolis.Transports.Tcp.Server` | Host TCP listener on a port |
| `Novolis.Transports.Tcp.Abstractions` | Middleware pipeline for handlers |
| `Novolis.Transports.Tcp.Cryptography` | AES payload encrypt/decrypt (auto-registered) |

