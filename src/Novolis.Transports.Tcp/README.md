# Novolis.Transports.Tcp

Transport-neutral adapters for TCP clients, listeners, and reliable ordered
streams.

This package is the compatibility implementation of the common transport
contracts. It does not replace the existing TCP middleware, server hosting, or
legacy payload-cryptography packages.

## Install

```bash
dotnet add package Novolis.Transports.Tcp
```

## Quick start

Use `TcpTransportConnection` for an already connected `NetworkStream`, or
`TcpTransportListener` when the host owns the listening socket. Treat TCP as a
reliable ordered stream and keep message framing and application
authentication in the protocol layer.
