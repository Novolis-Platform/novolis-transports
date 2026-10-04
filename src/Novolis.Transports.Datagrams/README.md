# Novolis.Transports.Datagrams

UDP datagram send and receive primitives plus bounded fragment reassembly.
Payload meaning, authentication, and reliability policy belong to the
consuming protocol.

## Install

```xml
<PackageReference Include="Novolis.Transports.Datagrams" Version="2026.1.*" />
```

## Usage

Create a `UdpDatagramChannel` with the local endpoint and send or receive
opaque payloads.
