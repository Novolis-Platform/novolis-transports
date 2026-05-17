using Novolis.Transports.Tcp.Cryptography;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Tcp.Server;

public static class WebHostBuilderExtensions
{
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