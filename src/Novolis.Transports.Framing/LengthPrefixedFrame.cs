namespace Novolis.Transports.Framing;

/// <summary>One payload carried by a length-prefixed stream frame.</summary>
public sealed record LengthPrefixedFrame(byte[] Payload);
