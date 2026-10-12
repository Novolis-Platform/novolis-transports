<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-transports/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-transports/) · [Source](https://github.com/Novolis-Platform/novolis-transports)
<!-- novolis-pkg-brand:end -->

# Novolis.Transports.Kad

Clean-room eMule Kad discovery and routing primitives over UDP. The package
contains 128-bit Kad identifiers, XOR distance, k-bucket routing, Kad packet
encoding, bootstrap, iterative lookup, and keyword/source publish helpers.

## Install

```bash
dotnet add package Novolis.Transports.Kad
```

## Quick start

Build a `KadRoutingTable` from the local node identity, then use the
resulting contacts to drive bootstrap and iterative lookup operations.

`Novolis.Transports.Kad` references `Novolis.Transports.Ed2k` for the shared
file identifier and tag representations. It does not share the BitTorrent
DHT implementation in `Novolis.Transports.Torrent`.

Kad protocol details vary across eMule-compatible clients. This first package
supports plain and zlib-packed UDP packets. Protocol obfuscation, buddy
callbacks, and modern UDP-key verification remain outside the initial surface.
