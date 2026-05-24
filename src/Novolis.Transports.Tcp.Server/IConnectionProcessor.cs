namespace Novolis.Transports.Tcp.Server;

/// <summary>Processes inbound TCP payload bytes and returns a response.</summary>
public interface IConnectionHandler
{
    /// <summary>Handles one received buffer and returns bytes to send back.</summary>
    /// <param name="input">Received payload.</param>
    /// <returns>Response payload.</returns>
    Task<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> input);
}
