# Novolis.Transports.Tailscale

Enumerates active Tailscale adapters and their IPv4 addresses. It does not
authenticate, create accounts, or implement a product protocol.

## Install

```xml
<PackageReference Include="Novolis.Transports.Tailscale" Version="2026.1.*" />
```

## Usage

Use `TailscaleAddressEnumerator` to select bind addresses before opening an
application listener.
