<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-transports/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-transports/) · [Source](https://github.com/Novolis-Platform/novolis-transports)
<!-- novolis-pkg-brand:end -->

# Novolis.Transports.Torrent.Trackers

HTTP clients and aggregation for maintained public BitTorrent tracker lists.
The package wraps:

- [newTrackon](https://newtrackon.com/) live and stable lists.
- [ngosang/trackerslist](https://github.com/ngosang/trackerslist) protocol-specific
  lists.
- [XIU2/TrackersListCollection](https://github.com/XIU2/TrackersListCollection)
  curated and protocol-specific lists.

## Install

```bash
dotnet add package Novolis.Transports.Torrent.Trackers
```

## Quick start

```csharp
using Novolis.Transports.Torrent.Trackers;

using var http = new HttpClient();
var catalog = new TorrentTrackerCatalog(http);

var stable = await catalog.GetDistinctTrackersAsync(
    TorrentTrackerListKind.Best,
    cancellationToken);

var live = await catalog.GetAsync(
    TorrentTrackerSourceKind.NewTrackon,
    TorrentTrackerListKind.Live,
    cancellationToken);
```

Pass a configured `HttpClient` so applications can provide timeouts, proxies,
headers, and certificate policy. The catalog only reads public newline-delimited
lists; it does not announce torrents or make tracker connections.
