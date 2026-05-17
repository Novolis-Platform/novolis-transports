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
