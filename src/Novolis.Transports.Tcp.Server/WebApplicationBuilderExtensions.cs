using Novolis.Transports.Tcp.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Tcp.Server;

/// <summary>Registers TCP connection handling on a <see cref="WebApplicationBuilder"/>.</summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>Configures Kestrel to listen on <paramref name="port"/> and use <typeparamref name="THandler"/>.</summary>
    /// <typeparam name="THandler">Per-connection handler.</typeparam>
    /// <param name="builder">Web application builder.</param>
    /// <param name="port">Listen port.</param>
    /// <returns><paramref name="builder"/> for chaining.</returns>
    public static WebApplicationBuilder UseTcpConnectionHandler<THandler>(this WebApplicationBuilder builder, int port)
        where THandler : class, IConnectionHandler
    {
        builder.Services.AddTransient<IConnectionHandler, THandler>();
        builder.Services.AddTcpPayloadEncryption();

        builder.WebHost.UseKestrelCore();
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP(port, listenOptions =>
            {
                listenOptions.UseConnectionHandler<TcpConnectionHandler>();
            });
        });

        return builder;
    }
}
