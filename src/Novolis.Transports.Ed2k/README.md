<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-transports/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-transports/) · [Source](https://github.com/Novolis-Platform/novolis-transports)
<!-- novolis-pkg-brand:end -->

# Novolis.Transports.Ed2k

Clean-room eDonkey2000/eMule transport primitives for .NET. The package
contains the eD2k binary codec, MD4 file identifiers, `ed2k://` links, server
lists, a server session, peer part verification, AICH, and the pairwise credit
ledger.

The package is MIT licensed and has no third-party dependencies. It targets
the protocol used by eMule and aMule. It does not contain a UI, an index
server, protocol obfuscation, or LowID callback hosting.

## Install

```bash
dotnet add package Novolis.Transports.Ed2k
```

## File identity

```csharp
using Novolis.Transports.Ed2k;

var fileId = Ed2kFileId.Compute(File.OpenRead(@"C:\shared\sample.bin"));
var link = Ed2kLink.CreateFile("sample.bin", new FileInfo(@"C:\shared\sample.bin").Length, fileId);
Console.WriteLine(link);
```

The eD2k part size is 9,728,000 bytes. MD4 is retained only for wire
compatibility with the eD2k network; it is not a suitable primitive for new
security decisions.

## Server session

`Ed2kSession` can connect to an endpoint, submit a search, and return the
server's result records. Use `ServerList.Load` for a user-supplied text list
or `server.met` file.

## Quick start

Start with `Ed2kFileId`, `Ed2kLink`, and the server/session types when adding
eD2k file identity or server discovery to an application.

This package is an independently authored protocol implementation. aMule and
eMule are GPL reference clients, not dependencies.
