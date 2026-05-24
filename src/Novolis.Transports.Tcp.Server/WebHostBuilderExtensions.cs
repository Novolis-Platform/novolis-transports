using Novolis.Transports.Tcp.Cryptography;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Tcp.Server;

/// <summary>Registers TCP connection handling on an <see cref="IWebHostBuilder"/>.</summary>
public static class WebHostBuilderExtensions
{
    /// <summary>Configures Kestrel to listen on <paramref name="port"/> and use <typeparamref name="THandler"/>.</summary>
    /// <typeparam name="THandler">Per-connection handler.</typeparam>
    /// <param name="builder">Web host builder.</param>
    /// <param name="port">Listen port.</param>
    /// <returns><paramref name="builder"/> for chaining.</returns>
    public static IWebHostBuilder UseTcpConnectionHandler<THandler>(this IWebHostBuilder builder, int port)
        where THandler : class, IConnectionHandler
    {
        builder.ConfigureServices(services =>
        {
            services.AddTransient<IConnectionHandler, THandler>();
            services.AddTcpPayloadEncryption();
        });

        builder.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(port, listenOptions =>
            {
                listenOptions.UseConnectionHandler<TcpConnectionHandler>();
            });
        });

        return builder;
    }
}
