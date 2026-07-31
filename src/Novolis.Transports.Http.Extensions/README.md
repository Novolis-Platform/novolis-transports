# Novolis.Transports.Http.Extensions

REST convenience extensions on `IRestClient`: typed JSON GET/POST/PUT/PATCH/DELETE and generic `SendAsync<T>` helpers using `System.Text.Json` with `JsonSerializerDefaults.Web`.

## Install

```bash
dotnet add package Novolis.Transports.Http.Extensions
```

Depends on `Novolis.Transports.Http.Abstractions`. Register the client via `Novolis.Transports.Http`.

## Quick start

```csharp
using Novolis.Transports.Http.Abstractions;
using Novolis.Transports.Http.Extensions;

IRestClient client = /* from DI */;

var user = await client.GetAsync<UserDto>("https://api.example.com/users/1");
var created = await client.PostAsync<CreateUserRequest, UserDto>(
    "https://api.example.com/users",
    new CreateUserRequest { Name = "Ada" });

await client.DeleteAsync("https://api.example.com/users/1");
```

## API

| Extension on `IRestClient` | Role |
|----------------------------|------|
| `GetAsync<T>`, `GetAsync` | GET with optional deserialization |
| `PostAsync<T>`, `PostAsync<TRequest, TResponse>` | POST |
| `PutAsync<T>`, `PutAsync<TRequest, TResponse>` | PUT |
| `PatchAsync<T>`, `PatchAsync<TRequest, TResponse>` | PATCH |
| `DeleteAsync<T>`, `DeleteAsync` | DELETE |
| `HeadAsync`, `OptionsAsync`, `TraceAsync` | Other verbs |
| `SendAsync<T>(HttpRequestMessage)` | Generic send + deserialize |

## Related

| Package | Role |
|---------|------|
| `Novolis.Transports.Http` | `AddNovolisHttp` DI setup |
| `Novolis.Transports.Http.Abstractions` | `IRestClient` contract |
| `Novolis.Transports.Http.Authentication` | Auth handlers for protected APIs |
