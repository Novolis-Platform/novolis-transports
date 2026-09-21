# Novolis.Transports.Discovery

Product-neutral UDP probe and beacon helpers. Applications define their own
probe token and interpret the beacon fields.

## Install

```xml
<PackageReference Include="Novolis.Transports.Discovery" Version="2026.1.*" />
```

## Usage

Use `DiscoveryScanner` for probes and `DiscoveryResponder` for generic beacon
responses.
