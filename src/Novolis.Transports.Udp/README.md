# Novolis.Transports.Udp

Bounded asynchronous UDP channels for discovery and explicitly packetized
real-time protocols.

UDP remains unordered and lossy. This package enforces a conservative payload
limit and bounded receive queue, but it does not silently add retransmission,
authentication, fragmentation, or congestion control.
