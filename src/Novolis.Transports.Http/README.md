# Novolis.Transports.Http

REST client factory and DI (`AddNovolisHttp`) with authentication and enricher pipelines.

## Install

```bash
dotnet add package Novolis.Transports.Http
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
services.AddNovolisHttp(b => b.AddAuthentication<MyAuth>());
var client = sp.GetRequiredService<IRestClient>();
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Transports.Http.Abstractions` | Contracts only |
| `Novolis.Transports.Http.Extensions` | `GetAsync` / `PostAsync` JSON helpers |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/getting-started.md)

## Support

Pre-release.
