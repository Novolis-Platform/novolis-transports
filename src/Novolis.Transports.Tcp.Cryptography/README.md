# Novolis.Transports.Tcp.Cryptography

AES payload encryption for Novolis TCP client/server. Registered automatically by `AddTcpClient` and TCP server setup; can also be registered explicitly.

## Install

Not published as a standalone NuGet package (`IsPackable=false`). Consumed transitively via `Novolis.Transports.Tcp.Client` / `.Server`.

## Quick start

```csharp
using Novolis.Transports.Tcp.Cryptography;

services.AddTcpPayloadEncryption(o =>
{
    o.Key = keyBytes;
    o.Iv = ivBytes;
});
```

## API

| Type | Role |
|------|------|
| `ITcpPayloadEncryptor` | Encrypt/decrypt byte payloads |
| `ITcpPayloadEncryptorFactory` | Factory for encryptors |
| `TcpPayloadEncryptor` / `TcpPayloadEncryptorFactory` | Default AES implementations |
| `TcpPayloadEncryptionOptions` | Key and IV configuration |
| `AesKey` | Record struct `(Key, Iv)` |
| `ServiceCollectionExtensions.AddTcpPayloadEncryption` | DI registration |

Obsolete `IAdvancedEncryptionService` / `AddAdvancedEncryption` aliases remain for migration.

## Related

| Package | Role |
|---------|------|
| `Novolis.Transports.Tcp.Client` | Auto-registers encryption |
| `Novolis.Transports.Tcp.Server` | Server-side handler host |
