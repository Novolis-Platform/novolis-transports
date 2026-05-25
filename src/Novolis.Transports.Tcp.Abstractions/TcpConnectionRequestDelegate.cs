namespace Novolis.Transports.Tcp.Abstractions;

/// <summary>Processes an inbound TCP payload and returns a response.</summary>
/// <param name="input">Received payload.</param>
/// <returns>Response payload.</returns>
public delegate ValueTask<ReadOnlyMemory<byte>> TcpConnectionRequestDelegate(ReadOnlyMemory<byte> input);
