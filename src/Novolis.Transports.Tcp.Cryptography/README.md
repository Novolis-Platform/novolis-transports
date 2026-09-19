# Novolis.Transports.Tcp.Cryptography

Legacy internal AES-CBC payload helper for Novolis TCP client/server.

## Security status

Do not use this component for new traffic, secure text, or an end-to-end protocol. It lacks
authenticated encryption, nonce lifecycle, replay protection, key agreement, peer identity, and
stream framing guarantees. Registration does not by itself protect a TCP send or receive path.

New secure text uses `Novolis.Security.SecureText` and `Novolis.Messaging.SecureText`, with
authenticated AES-GCM envelopes above the delivery transport.

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

## Related

| Package | Role |
|---------|------|
| `Novolis.Transports.Tcp.Client` | Auto-registers encryption |
| `Novolis.Transports.Tcp.Server` | Server-side handler host |
