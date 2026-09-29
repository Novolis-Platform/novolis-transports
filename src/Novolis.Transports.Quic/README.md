# Novolis.Transports.Quic

Secure QUIC connection, listener, and stream adapters over
`System.Net.Quic`.

.NET 10 exposes QUIC streams but not QUIC datagrams. Consumers must probe
support and use TCP or an explicitly authenticated UDP channel for
deadline-sensitive datagrams.

## Install

```bash
dotnet add package Novolis.Transports.Quic
```

## Quick start

Configure `QuicTransportOptions` and use `QuicTransportListener` or
`QuicTransportConnection` for authenticated, reliable streams. Probe
`QuicTransportCapabilities` before selecting QUIC for a deployment and retain
TCP or authenticated UDP as an explicit fallback where required.
