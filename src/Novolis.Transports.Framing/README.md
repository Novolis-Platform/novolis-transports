# Novolis.Transports.Framing

Bounded length-prefixed stream framing for protocol payloads. The package has
no knowledge of the messages carried inside a frame.

## Install

```xml
<PackageReference Include="Novolis.Transports.Framing" Version="2026.1.*" />
```

## Usage

Call `LengthPrefixedFrameCodec` around an existing reliable stream.
