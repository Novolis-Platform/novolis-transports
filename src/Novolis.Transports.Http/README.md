<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-transports">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Transports.Http

REST client factory and dependency injection for Novolis HTTP clients. Registers `IRestClient`, `IRestClientFactory`, and `HttpClient` with optional authentication and request enrichers.

## Install

```bash
dotnet add package Novolis.Transports.Http
```

Depends on `Novolis.Transports.Http.Abstractions`.

## Quick start

```csharp
using Novolis.Transports.Http;

services.AddNovolisHttp(
    auth => auth.AddAuthentication<BasicAuthentication>(),
    enrich => enrich.AddEnricher<MyRequestEnricher>());
```

Vanilla client (no auth/enrichers):

```csharp
services.AddNovolisHttp();
var client = sp.GetRequiredService<IRestClientFactory>().CreateClient(vanilla: true);
```

Use `Novolis.Transports.Http.Extensions` for typed REST helpers (`GetAsync<T>`, `PostAsync<T>`, etc.).

## API

| Type | Role |
|------|------|
| `ServiceCollectionExtensions.AddNovolisHttp` | Overloads with auth/enricher builders |
| `AddNovolisHttpAuthentication<T>` | Register single auth handler |
| `AddNovolisHttpRequestEnricher<T>` | Register single enricher |
| `RestClient` | Default `IRestClient` |
| `RestClientFactory` | Default `IRestClientFactory` |
| `AuthenticationBuilder` | Fluent auth registration |
| `EnricherBuilder` | Fluent enricher registration |

Obsolete `AddFrankHttp*` aliases remain for migration.

## Related

| Package | Role |
|---------|------|
| `Novolis.Transports.Http.Abstractions` | `IRestClient`, `IHttpAuthentication`, `IRequestEnricher` |
| `Novolis.Transports.Http.Authentication` | Basic, API key, OIDC auth implementations |
| `Novolis.Transports.Http.Extensions` | REST convenience extensions |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/getting-started.md)

