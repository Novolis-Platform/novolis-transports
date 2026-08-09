# Release

This repository publishes with the org CalVer scheme (`2026.1.*`) via `merge.yml` to GitHub Packages when packages are packable.

See [release-policy](https://github.com/Novolis-Platform/novolis-governance/blob/main/docs/release-policy.md).

Published docs: [https://novolis-platform.github.io/.github/novolis-transports/](https://novolis-platform.github.io/.github/novolis-transports/)

## Packages

- `Novolis.Transports.Http`
- `Novolis.Transports.Http.Abstractions`
- `Novolis.Transports.Http.Authentication`
- `Novolis.Transports.Http.Extensions`
- `Novolis.Transports.LocalIpc`
- `Novolis.Transports.Tcp.Abstractions`
- `Novolis.Transports.Tcp.Client`
- `Novolis.Transports.Tcp.Server`
- `Novolis.Transports.Torrent`
- `Novolis.Transports.WireFish`

## Consumers

Restore from nuget.org + `https://nuget.pkg.github.com/Novolis-Platform/index.json` only.

Local multi-repo iteration: open `d:\novolis\Novolis.Platform.slnx` (ProjectReference mode) — do not add a local feed.
