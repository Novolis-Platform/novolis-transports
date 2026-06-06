namespace Novolis.Transports.LocalIpc;

/// <summary>Length-prefixed frame exchanged over a local IPC connection.</summary>
public sealed record LocalIpcFrame(
    long Sequence,
    string Kind,
    string Name,
    byte[] Payload);
