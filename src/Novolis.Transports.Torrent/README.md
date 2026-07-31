# Novolis.Transports.Torrent

BitTorrent 1.0 peer client for the Novolis transports stack.

## Install

```bash
dotnet add package Novolis.Transports.Torrent
```

## Quick start

```csharp
using Novolis.Transports.Torrent;

if (!TorrentInfo.TryLoad(@"TinyCore.iso.torrent", out var info) || info is null)
    throw new InvalidOperationException("Bad torrent.");

using var client = new TorrentClient(listeningPort: 6881, baseDirectory: @"D:\downloads");
client.Start();
client.Start(info);

var progress = client.GetProgressInfo(info.InfoHash);
client.Stop(info.InfoHash);
```

## Provenance

Ported from `Frank.TorrentClient` (MIT; Aljaz Simonic / Frank R. Haugen). Legacy `DefensiveProgrammingFramework` replaced with a net10-local guard shim. Search scrapers and GUIs are not included.

`GetProgressInfo` reports **0–100** percent complete.
