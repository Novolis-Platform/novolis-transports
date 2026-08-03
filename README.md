<!-- novolis-marketing:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-brand-transparent.svg" width="360" alt="Novolis"/>
  </a>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/banners/novolis-transports.svg" width="100%" alt="novolis-transports"/>
</p>

<p align="center">
  <strong>HTTP, IPC, torrents, and more</strong><br/>
  Transport libraries: HTTP, local IPC, torrent, and related adapters.
</p>

<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-transports/actions"><img src="https://img.shields.io/github/actions/workflow/status/Novolis-Platform/novolis-transports/merge.yml?branch=main&label=merge&logo=github" alt="merge"/></a>
  <a href="https://github.com/orgs/Novolis-Platform/packages?repo_name=novolis-transports"><img src="https://img.shields.io/badge/packages-GitHub%20Packages-0a7ea3?logo=nuget" alt="packages"/></a>
  <a href="https://github.com/Novolis-Platform"><img src="https://img.shields.io/badge/org-Novolis--Platform-111827" alt="org"/></a>
</p>

<p align="center">
  <a href="https://nuget.pkg.github.com/Novolis-Platform/index.json"><code>https://nuget.pkg.github.com/Novolis-Platform/index.json</code></a>
  ·
  <a href="https://github.com/Novolis-Platform/.github/blob/main/profile/README.md">Org landing</a>
  ·
  <a href="https://github.com/Novolis-Platform/novolis-governance">Governance</a>
</p>

---
<!-- novolis-marketing:end -->
<!-- novolis-package-index:start -->
> **GitHub Packages shows this repository README on every package page** (upstream limitation).
> Open the **package README** for install and quick start — embedded in each .nupkg and linked below.

## Published packages

| Package | Install | Package README |
|---------|---------|----------------|
| `Novolis.Transports.Http` | `dotnet add package Novolis.Transports.Http` | [README](https://github.com/Novolis-Platform/novolis-transports/blob/main/src/Novolis.Transports.Http/README.md) |
| `Novolis.Transports.Http.Abstractions` | `dotnet add package Novolis.Transports.Http.Abstractions` | [README](https://github.com/Novolis-Platform/novolis-transports/blob/main/src/Novolis.Transports.Http.Abstractions/README.md) |
| `Novolis.Transports.Http.Authentication` | `dotnet add package Novolis.Transports.Http.Authentication` | [README](https://github.com/Novolis-Platform/novolis-transports/blob/main/src/Novolis.Transports.Http.Authentication/README.md) |
| `Novolis.Transports.Http.Extensions` | `dotnet add package Novolis.Transports.Http.Extensions` | [README](https://github.com/Novolis-Platform/novolis-transports/blob/main/src/Novolis.Transports.Http.Extensions/README.md) |
| `Novolis.Transports.LocalIpc` | `dotnet add package Novolis.Transports.LocalIpc` | [README](https://github.com/Novolis-Platform/novolis-transports/blob/main/src/Novolis.Transports.LocalIpc/README.md) |
| `Novolis.Transports.Tcp.Abstractions` | `dotnet add package Novolis.Transports.Tcp.Abstractions` | [README](https://github.com/Novolis-Platform/novolis-transports/blob/main/src/Novolis.Transports.Tcp.Abstractions/README.md) |
| `Novolis.Transports.Tcp.Client` | `dotnet add package Novolis.Transports.Tcp.Client` | [README](https://github.com/Novolis-Platform/novolis-transports/blob/main/src/Novolis.Transports.Tcp.Client/README.md) |
| `Novolis.Transports.Tcp.Server` | `dotnet add package Novolis.Transports.Tcp.Server` | [README](https://github.com/Novolis-Platform/novolis-transports/blob/main/src/Novolis.Transports.Tcp.Server/README.md) |
| `Novolis.Transports.Torrent` | `dotnet add package Novolis.Transports.Torrent` | [README](https://github.com/Novolis-Platform/novolis-transports/blob/main/src/Novolis.Transports.Torrent/README.md) |
| `Novolis.Transports.WireFish` | `dotnet add package Novolis.Transports.WireFish` | [README](https://github.com/Novolis-Platform/novolis-transports/blob/main/src/Novolis.Transports.WireFish/README.md) |

For NuGet.org and Visual Studio, the **embedded** README.md inside each package is authoritative.

<!-- novolis-package-index:end -->
# Transports

TCP and HTTP client libraries for the Novolis platform.

## Packages

| Package | Purpose |
|---------|---------|
| `Novolis.Transports.Tcp.Client` | TCP client |
| `Novolis.Transports.Tcp.Server` | TCP server hosting |
| `Novolis.Transports.Http` | REST client factory and DI (`AddNovolisHttp`) |
| `Novolis.Transports.Http.Abstractions` | HTTP authentication and enricher contracts |
| `Novolis.Transports.Http.Authentication` | Basic, API key, and OIDC client auth |
| `Novolis.Transports.Http.Extensions` | REST convenience extensions |
| `Novolis.Transports.WireFish` | Live packet capture (SharpPcap) via `Novolis.Messaging.Channels` |
| `Novolis.Transports.LocalIpc` | Framed local IPC over named pipes and Unix domain sockets |

`Novolis.Transports.Tcp.Cryptography` provides internal TCP payload AES helpers (`AddTcpPayloadEncryption`).

`Novolis.Transports.LocalIpc` is the reusable transport layer used by the live audio host/client stack. It is intentionally domain-agnostic so other Novolis apps can reuse the same framed request/response and event streaming model.

## Install

```bash
dotnet add package Novolis.Transports.Http --version 0.1.0-preview.1
```

## Quick start

```csharp
services.AddNovolisHttp(b => b.AddAuthentication<MyAuth>());

services.AddNovolisWireFish(
    w => w.AddPacketHandler<MyPacketHandler>(),
    o => o.BpfFilter = "tcp port 443");
```

Legacy `AddFrankHttp` and `Frank.WireFish` APIs remain as obsolete aliases.

## Documentation

- [Getting started](docs/getting-started.md)
- [Design](docs/design.md)
- [Release](docs/release.md)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## Security

See [SECURITY.md](SECURITY.md).

