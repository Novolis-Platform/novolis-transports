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
| `Novolis.Transports.Tcp.Client` | `dotnet add package Novolis.Transports.Tcp.Client` | [README](https://github.com/Novolis-Platform/novolis-transports/blob/main/src/Novolis.Transports.Tcp.Client/README.md) |
| `Novolis.Transports.Tcp.Server` | `dotnet add package Novolis.Transports.Tcp.Server` | [README](https://github.com/Novolis-Platform/novolis-transports/blob/main/src/Novolis.Transports.Tcp.Server/README.md) |
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

`Novolis.Transports.Tcp.Cryptography` provides internal TCP payload AES helpers (`AddTcpPayloadEncryption`).

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

