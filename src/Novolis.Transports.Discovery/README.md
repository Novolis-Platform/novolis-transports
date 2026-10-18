# Novolis.Transports.Discovery

Cohort-aware service discovery for local and routed networks. The package
provides:

- UDP probe and beacon helpers for zero-configuration LAN discovery.
- DNS SRV/TXT resolution for services published by managed DNS or Tailscale
  DNS.
- An HTTP(S) lease-registry client for routed networks and coordinated
  environments.
- A thread-safe `ServiceDiscoveryMap` that merges results from any source.

## Install

```xml
<PackageReference Include="Novolis.Transports.Discovery" Version="2026.1.*" />
```

## Cohort map

Every registration has a `cohort`, `serviceName`, and `instanceId`. A caller
can merge multiple sources without making the product protocol part of this
package:

```csharp
using Novolis.Transports.Discovery;

var query = new ServiceDiscoveryQuery("reach", "host");
var map = new ServiceDiscoveryMap();

var dns = new DnsSrvServiceDiscovery(
    new DnsSrvServiceDiscoveryOptions("example.internal"));
var services = await dns.DiscoverAsync(query, cancellationToken);

map.Replace(services);
var hosts = map.Find(query);
```

`ServiceDiscoveryMap` is deliberately a snapshot map. Use `RefreshAsync` for a
one-shot concurrent query of multiple sources, then refresh periodically,
subscribe to a registry stream, or combine DNS with another source as needed.

## DNS SRV/TXT

For the query above, the resolver asks for:

```text
_host._tcp.reach.example.internal
```

Each SRV record becomes one service instance. TXT values at the same name use
`key=value` syntax and become instance metadata:

```text
_host._tcp.reach.example.internal. 60 IN SRV 0 0 19800 host-a.example.internal.
_host._tcp.reach.example.internal. 60 IN TXT "hostName=host-a" "protocolVersion=1.0"
```

DNS publishing is intentionally left to the network's DNS or service
advertisement system. The resolver works with ordinary DNS servers and does
not require a product-specific coordinator.

## HTTP(S) lease registry

`HttpServiceRegistry` uses an intentionally small REST contract:

| Method | Path | Purpose |
| --- | --- | --- |
| `PUT` | `/v1/registrations/{cohort}/{service}/{instance}` | create or renew a lease |
| `DELETE` | same registration path | remove a lease |
| `GET` | `/v1/services?cohort=...&serviceName=...` | list live registrations |

The `PUT` body contains the registration plus `leaseDurationSeconds`. The
registry removes entries after the lease expires, so a stopped process does
not remain in the cohort map indefinitely.

```csharp
using var http = new HttpClient
{
    BaseAddress = new Uri("https://registry.example.internal/"),
};
var registry = new HttpServiceRegistry(
    http,
    http.BaseAddress);

var registration = new ServiceRegistration(
    "reach",
    "host",
    Environment.MachineName,
    new[] { new ServiceEndpoint(new Uri("tcp://10.0.0.25:19800")) },
    new Dictionary<string, string>
    {
        ["hostName"] = Environment.MachineName,
        ["protocolVersion"] = "1.0",
    });

var session = new ServiceRegistrationSession(
    registry,
    registration,
    leaseDuration: TimeSpan.FromSeconds(30),
    renewalPeriod: TimeSpan.FromSeconds(10));
await session.RunAsync(cancellationToken);
```

Configure authentication and certificate validation on the supplied
`HttpClient`; the library does not embed credentials.

## UDP compatibility

Use `DiscoveryScanner` for probes and `DiscoveryResponder` for generic beacon
responses. These primitives remain available for products such as Reach that
need zero-configuration broadcast discovery on a trusted LAN.
