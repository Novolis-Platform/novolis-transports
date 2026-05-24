# Novolis.Transports.Http.Authentication

HTTP authentication handlers: Basic, API key header, and OIDC client credentials.

## Install

```bash
dotnet add package Novolis.Transports.Http.Authentication
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
services.Configure<BasicAuthenticationConfiguration>(o =>
{
    o.Username = "user";
    o.Password = "secret";
});
services.AddSingleton<IHttpAuthentication, BasicAuthentication>();
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Transports.Http` | Client factory and `AddNovolisHttp` |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/getting-started.md)

## Support

Pre-release.
