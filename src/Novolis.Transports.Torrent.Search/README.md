<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-transports/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-transports/) · [Source](https://github.com/Novolis-Platform/novolis-transports)
<!-- novolis-pkg-brand:end -->

# Novolis.Transports.Torrent.Search

Provider-neutral torrent search contracts with public Magnetz and Torznab
adapters. Magnetz supplies no-auth public metadata search. Torznab lets the
application select and configure its own indexer or indexer aggregator
(such as Jackett or Prowlarr) without coupling this library to a search-site
catalogue.

Results normalize:

- names, magnet and torrent URLs, info hashes, and sizes;
- seeders, leechers, completed downloads, and source-provided availability;
- provider ratios plus a computed seeder-to-leecher ratio; and
- categories, publication time, and provider-specific attributes.

## Install

```bash
dotnet add package Novolis.Transports.Torrent.Search
```

## Quick start

For public metadata search:

```csharp
using Novolis.Transports.Torrent.Search;

using var http = new HttpClient();
var provider = new MagnetzSearchProvider(http);
var search = new TorrentSearchClient(provider);
var results = await search.SearchAsync(new TorrentSearchQuery("ubuntu"));
```

For a caller-configured Torznab endpoint:

```csharp
using Novolis.Transports.Torrent.Search;

using var http = new HttpClient();
var provider = new TorznabSearchProvider(
    http,
    new Uri("http://127.0.0.1:9117/api/v2.0/indexers/all/results/torznab/api"),
    apiKey: "configured-by-the-host");
var search = new TorrentSearchClient(provider);

var results = await search.SearchAsync(
    new TorrentSearchQuery("ubuntu")
    {
        MinimumSeeders = 5,
        MinimumLeechers = 1,
        MinimumRatio = 2m,
        MinimumAvailability = 1m,
        SortBy = TorrentSearchSort.Seeders,
    },
    cancellationToken);
```

When a provider does not report a requested metric, a result is excluded by
that metric's filter rather than being treated as a match. Configure the
`HttpClient` with the endpoint's authentication, timeout, proxy, and
certificate policy.
