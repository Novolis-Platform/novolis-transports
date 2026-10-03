# Getting started

Transport libraries: local IPC, TCP, torrent, and related adapters. For HTTP request documents and execution, see [novolis-http](https://github.com/Novolis-Platform/novolis-http).

Published guide: [https://novolis-platform.github.io/.github/novolis-transports/](https://novolis-platform.github.io/.github/novolis-transports/)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- GitHub Packages auth for `Novolis.*` (see [nuget-only-policy](https://github.com/Novolis-Platform/novolis-governance/blob/main/docs/nuget-only-policy.md))

Configure GPR once from a sibling `novolis-governance` checkout:

```powershell
pwsh -File d:\novolis\novolis-governance\scripts\configure-gpr-user-nuget.ps1
```

Local multi-repo iteration uses ProjectReference mode via `d:\novolis\Novolis.Platform.slnx` — never a local NuGet folder feed.

## Next

- [design.md](design.md) — layer placement and non-goals
- [release.md](release.md) — publish cadence
- [Org docs catalog](https://novolis-platform.github.io/.github/)
