namespace Novolis.Transports.Tcp.Client;

/// <summary>Options for <see cref="TcpClient"/> read timeouts.</summary>
public class TcpClientOptions
{
    /// <summary>Maximum time to wait for response bytes after sending a payload.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
}
