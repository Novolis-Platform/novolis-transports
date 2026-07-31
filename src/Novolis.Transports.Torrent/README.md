# Novolis.Transports.Torrent

BitTorrent client, metadata parsing, and torrent file creation. Large protocol surface (BEncoding, peer wire, tracker HTTP/UDP) for seeder/leecher scenarios.

## Install

```bash
dotnet add package Novolis.Transports.Torrent
```

## Quick start

```csharp
using Novolis.Transports.Torrent;

if (!TorrentInfo.TryLoad(@"C:\torrents\sample.torrent", out var info))
    throw new InvalidOperationException("Invalid torrent file.");

using var client = new TorrentClient(listenPort: 6881, downloadDirectory: @"C:\downloads");
client.Start(info);

var progress = client.GetProgressInfo(info.InfoHash); // 0–100%
client.Stop(info.InfoHash);
```

Create a torrent file:

```csharp
TorrentCreator.Create(/* options */);
```

## API

| Type | Role |
|------|------|
| `TorrentClient` | `Start()`, `Start(TorrentInfo)`, `Stop`, `GetProgressInfo` |
| `TorrentInfo` | `TryLoad(path|bytes)`, metadata properties |
| `TorrentCreator` | Static torrent file creation |
| `TorrentProgressInfo` / `TorrentPeerInfo` / `TorrentTrackerInfo` | Progress DTOs |
| `TorrentClientException`, `BEncodingException`, … | Error types |

## Dogfooding / apps

Used by `Novolis.Avalonia.Controls.TorrentSessionPanel` and **TorrentLab** smoke tests in `novolis-dogfooding`.

## Related

| Package | Role |
|---------|------|
| `Novolis.Transports.Http` | HTTP tracker communication helpers |
| `Novolis.Avalonia.Controls` | UI torrent session panel |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/getting-started.md)
