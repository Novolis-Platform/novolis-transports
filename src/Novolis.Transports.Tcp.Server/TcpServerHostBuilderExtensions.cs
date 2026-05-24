using Novolis.Transports.Tcp.Cryptography;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Novolis.Transports.Tcp.Server;

/// <summary>Configures Kestrel to accept raw TCP connections.</summary>
public static class TcpServerHostBuilderExtensions
{
    /// <summary>Registers <typeparamref name="THandler"/> and listens on <paramref name="port"/>.</summary>
    /// <typeparam name="THandler">Per-connection handler.</typeparam>
    /// <param name="hostBuilder">Host builder.</param>
    /// <param name="port">Listen port.</param>
    /// <returns><paramref name="hostBuilder"/> for chaining.</returns>
    public static IHostBuilder UseTcpConnectionHandler<THandler>(this IHostBuilder hostBuilder, int port)
        where THandler : class, IConnectionHandler
    {
        hostBuilder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.ConfigureServices(services =>
            {
                services.AddTransient<IConnectionHandler, THandler>();
                services.AddTcpPayloadEncryption();
            });

            webBuilder.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(port, listenOptions =>
                {
                    listenOptions.UseConnectionHandler<TcpConnectionHandler>();
                });
            });
        });

        return hostBuilder;
    }
}
