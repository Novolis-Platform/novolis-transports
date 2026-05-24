using System.Net;

namespace Novolis.Transports.Tcp.Client;

/// <summary>Simple request/response TCP client.</summary>
public interface ITcpClient
{
    /// <summary>Sends bytes to a server and reads the response (subject to timeout).</summary>
    /// <param name="serverIp">Server IP address.</param>
    /// <param name="serverPort">Server port.</param>
    /// <param name="data">Payload to send.</param>
    /// <returns>Response bytes (may be empty on timeout or error).</returns>
    Task<ReadOnlyMemory<byte>> SendAsync(IPAddress serverIp, int serverPort, ReadOnlyMemory<byte> data);
}
