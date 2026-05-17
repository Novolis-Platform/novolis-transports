namespace Novolis.Transports.Tcp.Server;

public interface IConnectionHandler
{
    Task<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> input);
}