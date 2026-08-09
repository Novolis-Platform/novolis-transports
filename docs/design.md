# Design

Transport libraries: HTTP, local IPC, torrent, and related adapters.

Published docs: [https://novolis-platform.github.io/.github/novolis-transports/](https://novolis-platform.github.io/.github/novolis-transports/)

## Layer placement

Follow [library-boundaries](https://github.com/Novolis-Platform/novolis-governance/blob/main/docs/library-boundaries.md) for layer placement.

## Goals

- Keep public APIs documented and packable as `Novolis.*` on GitHub Packages (when applicable).
- Prefer BCL types and existing Novolis packages over parallel abstractions.
- Document restore and ProjectReference-mode builds without local NuGet folder feeds.

## Non-goals

- Local NuGet folder feeds or committed cross-repo `ProjectReference` into sibling checkouts.
- Avalonia package references outside `Novolis.Avalonia.*`.
- Upward spine dependencies (e.g. Math → Simulation).

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

## Topics

- `dotnet`
- `networking`
- `novolis`
