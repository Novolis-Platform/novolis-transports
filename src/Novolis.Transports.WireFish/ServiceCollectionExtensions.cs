using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Novolis.Messaging.Channels;
using Novolis.Transports.WireFish.Internals;

namespace Novolis.Transports.WireFish;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNovolisWireFish(
        this IServiceCollection services,
        Action<IWireFishBuilder> configure)
        => services.AddNovolisWireFish(configure, _ => { });

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
