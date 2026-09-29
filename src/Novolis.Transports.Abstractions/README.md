# Novolis.Transports.Abstractions

Transport-neutral contracts for reliable streams, datagrams, connection
lifecycle, endpoints, capabilities, and diagnostics.

Implementations such as TCP, UDP, and QUIC depend on this package. It has no
dependency on a product protocol, UI framework, or operating-system host.

## Install

```bash
dotnet add package Novolis.Transports.Abstractions
```

## Quick start

Reference this package when an application or library should depend on
transport-neutral connection, stream, datagram, and capability contracts.
Choose a concrete transport package separately at the host or composition
boundary.
