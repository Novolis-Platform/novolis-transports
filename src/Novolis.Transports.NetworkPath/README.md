# Novolis.Transports.NetworkPath

Cross-platform usable-adapter inventory, route resolution, and explicit bound
TCP connection probes for .NET applications.

The package reports which local source address the operating system selects
for a destination, classifies adapters with evidence, and can compare
repeated probes bound to individual adapters. A TCP-connect probe is not an
ICMP ping and measures the reachability and handshake time of the selected
host and port. Interfaces without a local address or gateway are omitted from
the default usable-adapter inventory because they cannot carry a bound probe.

Tunnel and VPN detection is heuristic. An adapter name, interface type, or
description can make an adapter a `TunnelCandidate`, but the package does not
claim to identify a VPN with certainty and does not contact an IP-reputation
service.

The package does not change system routes, launch operating-system commands,
or discover a public address unless an application supplies an explicit
external-address provider.
