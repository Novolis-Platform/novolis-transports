# Novolis.Transports.Http.Abstractions

HTTP client, authentication, and request-enricher contracts for the Novolis HTTP transport stack.

## Install

```bash
dotnet add package Novolis.Transports.Http.Abstractions
```

Contracts only — register implementations via `Novolis.Transports.Http` and `Novolis.Transports.Http.Authentication`.

## Quick start

```csharp
using Novolis.Transports.Http.Abstractions;

public sealed class MyAuth : IHttpAuthentication
{
    public Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return Task.CompletedTask;
    }
}

public sealed class MyEnricher : IRequestEnricher
{
    public void Enrich(HttpRequestMessage request) =>
        request.Headers.Add("X-Client", "Novolis");
}
```

## API

| Type | Role |
|------|------|
| `IRestClient` | `SendAsync(HttpRequestMessage, CancellationToken)` |
| `IRestClientFactory` | `CreateClient(vanilla?)`, `CreateClient(enrichers, authentications)` |
| `IHttpAuthentication` | `AuthenticateAsync(request, CancellationToken)` |
| `IRequestEnricher` | `Enrich(request)` |
| `IAuthenticationBuilder` | `AddAuthentication<T>()`, `AddAuthentication<T>(instance)` |
| `IEnricherBuilder` | `AddEnricher<T>()`, `AddEnricher<T>(instance)` |

## Related

| Package | Role |
|---------|------|
| `Novolis.Transports.Http` | DI registration (`AddNovolisHttp`) |
| `Novolis.Transports.Http.Authentication` | Built-in auth handlers |
| `Novolis.Transports.Http.Extensions` | Typed REST helpers on `IRestClient` |
