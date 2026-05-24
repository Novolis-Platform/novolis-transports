# Novolis.Transports.Tcp.Cryptography

AES encrypt/decrypt helpers for TCP payloads (`AddTcpPayloadEncryption`).

## Install

```bash
dotnet add package Novolis.Transports.Tcp.Cryptography
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
services.AddTcpPayloadEncryption(o =>
{
    o.Key = Convert.ToBase64String(keyBytes);
    o.Iv = Convert.ToBase64String(ivBytes);
});
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Transports.Tcp.Client` | Registers encryption with the client |
| `Novolis.Transports.Tcp.Server` | Registers encryption with the server |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-transports/blob/main/docs/getting-started.md)

## Support

Pre-release. Replace default key material before production use.
