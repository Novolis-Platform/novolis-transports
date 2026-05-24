using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Novolis.Messaging.Channels;
using Novolis.Transports.WireFish.Internals;

namespace Novolis.Transports.WireFish;

/// <summary>DI registration for live packet capture (WireFish).</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers WireFish capture pipeline and configures handlers.</summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Handler registration callback.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddNovolisWireFish(
        this IServiceCollection services,
        Action<IWireFishBuilder> configure)
        => services.AddNovolisWireFish(configure, _ => { });

    /// <summary>Registers WireFish capture pipeline with handlers and options.</summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Handler registration callback.</param>
    /// <param name="configureOptions">Capture options callback.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddNovolisWireFish(
        this IServiceCollection services,
        Action<IWireFishBuilder> configure,
        Action<WireFishOptions> configureOptions)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<WireFishOptions>>(
            new ConfigureOptions<WireFishOptions>(configureOptions)));
        services.AddOptions<WireFishOptions>();
        services.AddPacketCapturePipeline();
        var builder = new WireFishBuilder(services);
        configure(builder);
        return services;
    }

    /// <summary>Registers channel, hosted capture services, and handler dispatch.</summary>
    /// <param name="services">Service collection.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
    internal static IServiceCollection AddPacketCapturePipeline(this IServiceCollection services)
    {
        services.AddChannel<DevicePacket>();
        services.AddHostedService<PacketCaptureService>();
        services.AddSingleton<PacketHandler>();
        services.AddSingleton<InterfaceProvider>();
        services.AddHostedService<DevicePacketHandler>();
        return services;
    }
}
