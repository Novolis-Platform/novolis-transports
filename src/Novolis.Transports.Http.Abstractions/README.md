# Novolis.Transports.Http.Abstractions

Contracts for HTTP REST clients, authentication, and request enrichers.

## Install

```bash
dotnet add package Novolis.Transports.Http.Abstractions
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

Implement `IHttpAuthentication` and `IRestClient`, or reference `Novolis.Transports.Http` for default DI wiring.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Transports.Http` | `AddNovolisHttp` and default `RestClient` |
| `Novolis.Transports.Http.Authentication` | Basic, API key, OIDC handlers |
| `Novolis.Transports.Http.Extensions` | JSON verb helpers |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/design.md)

## Support

Pre-release.
