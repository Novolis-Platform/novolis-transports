using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Novolis.Transports.Tcp.Server;

/// <summary>Host factory for Kestrel-based TCP servers.</summary>
public static class Server
{
    /// <summary>Creates a generic host that listens on <paramref name="port"/> and dispatches to <typeparamref name="THandler"/>.</summary>
    /// <typeparam name="THandler">Connection handler implementation.</typeparam>
    /// <param name="port">TCP listen port.</param>
    /// <param name="configureServices">Optional additional service configuration.</param>
    /// <returns>Built and ready-to-run host.</returns>
    public static IHost CreateTcpServer<THandler>(int port, Action<HostBuilderContext, IServiceCollection>? configureServices = null)
        where THandler : class, IConnectionHandler
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices(configureServices ?? ((context, services) => { }))
            .UseTcpConnectionHandler<THandler>(port)
            .Build();
    }
}
