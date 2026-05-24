# Novolis.Transports.Http.Extensions

JSON and HTTP verb extension methods for `IRestClient`.

## Install

```bash
dotnet add package Novolis.Transports.Http.Extensions
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
var dto = await client.GetAsync<MyDto>("/api/items", cancellationToken);
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Transports.Http` | Core client and DI |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/getting-started.md)

## Support

Pre-release.
