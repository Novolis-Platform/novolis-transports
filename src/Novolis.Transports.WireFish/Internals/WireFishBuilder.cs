using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Novolis.Transports.WireFish.Internals;

internal class WireFishBuilder(IServiceCollection services) : IWireFishBuilder
{
    /// <inheritdoc />
    public IWireFishBuilder AddPacketHandler<THandler>() where THandler : class, IPacketHandler
    {
        if (services.All(x => x.ServiceType != typeof(THandler)))
            services.AddSingleton<THandler>();

        // Forward IPacketHandler → the same THandler singleton (not a second instance).
        // TryAddEnumerable keys on (ServiceType, ImplementationType) so re-entry is idempotent
        // and multiple distinct handler types can co-exist.
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IPacketHandler, THandler>(
                sp => sp.GetRequiredService<THandler>()));

        return this;
    }
}
