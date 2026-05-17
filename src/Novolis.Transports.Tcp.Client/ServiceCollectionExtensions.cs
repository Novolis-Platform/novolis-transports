using Novolis.Transports.Tcp.Cryptography;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Tcp.Client;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a <see cref="ITcpClient"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddTcpClient(this IServiceCollection services, Action<TcpClientOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<ITcpClient, TcpClient>();
        services.AddTcpPayloadEncryption();
        return services;
    }
    
    /// <summary>
    /// Adds a <see cref="ITcpClient"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddTcpClient(this IServiceCollection services)
    {
        services.AddSingleton<ITcpClient, TcpClient>();
        services.AddTcpPayloadEncryption();
        return services;
    }
}