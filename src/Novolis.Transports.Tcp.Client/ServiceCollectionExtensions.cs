using Novolis.Transports.Tcp.Cryptography;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Tcp.Client;

/// <summary>DI registration for <see cref="ITcpClient"/>.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="TcpClient"/> with configured options and TCP payload encryption helpers.</summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Options callback.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddTcpClient(this IServiceCollection services, Action<TcpClientOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<ITcpClient, TcpClient>();
        services.AddTcpPayloadEncryption();
        return services;
    }

    /// <summary>Registers <see cref="TcpClient"/> with default options and TCP payload encryption helpers.</summary>
    /// <param name="services">Service collection.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddTcpClient(this IServiceCollection services)
    {
        services.AddSingleton<ITcpClient, TcpClient>();
        services.AddTcpPayloadEncryption();
        return services;
    }
}
