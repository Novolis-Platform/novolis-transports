<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-transports">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Transports.Http.Authentication

Built-in `IHttpAuthentication` implementations: Basic, API key, and OIDC bearer token injection.

## Install

```bash
dotnet add package Novolis.Transports.Http.Authentication
```

Depends on `Novolis.Transports.Http.Abstractions`. Wire through `AddNovolisHttp` in `Novolis.Transports.Http`.

## Quick start

```csharp
using Novolis.Transports.Http;
using Novolis.Transports.Http.Authentication;

services.AddNovolisHttp(auth => auth.AddAuthentication<BasicAuthentication>());
```

Configure options when constructing or via DI:

```csharp
new BasicAuthentication(new BasicAuthenticationConfiguration
{
    Username = "user",
    Password = "pass",
});

new ApiKeyAuthentication(new ApiKeyAuthenticationConfiguration
{
    HeaderName = "X-Api-Key",
    ApiKey = "secret",
});

new OidcAuthentication(new OidcAuthenticationConfiguration { /* ... */ }, tokenProvider);
```

## API

| Type | Role |
|------|------|
| `BasicAuthentication` / `BasicAuthenticationConfiguration` | HTTP Basic auth |
| `ApiKeyAuthentication` / `ApiKeyAuthenticationConfiguration` | Header-based API key |
| `OidcAuthentication` / `OidcAuthenticationConfiguration` | OIDC bearer tokens |
| `OidcTokenProvider` | Default `IOidcTokenProvider` |
| `IOidcTokenProvider` | Token acquisition port |
| `IOicdTokenProvider` | Obsolete typo alias → `IOidcTokenProvider` |

## Related

| Package | Role |
|---------|------|
| `Novolis.Transports.Http.Abstractions` | `IHttpAuthentication` contract |
| `Novolis.Transports.Http` | DI registration |
| `Novolis.Transports.Http.Extensions` | Typed REST calls after auth |

