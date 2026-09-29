# Novolis.Transports.Udp

Bounded asynchronous UDP channels for discovery and explicitly packetized
real-time protocols.

UDP remains unordered and lossy. This package enforces a conservative payload
limit and bounded receive queue, but it does not silently add retransmission,
authentication, fragmentation, or congestion control.

## Install

```bash
dotnet add package Novolis.Transports.Udp
```

## Quick start

Create `UdpDatagramChannel` with `UdpDatagramChannelOptions`, then use the
bounded send and receive operations for discovery or an explicitly packetized
real-time protocol. Add authentication, sequencing, loss recovery, and
fragmentation in the protocol that owns the datagrams.
