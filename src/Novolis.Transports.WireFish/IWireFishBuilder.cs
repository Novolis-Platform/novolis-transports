namespace Novolis.Transports.WireFish;

/// <summary>Fluent registration of WireFish packet handlers.</summary>
public interface IWireFishBuilder
{
    /// <summary>Registers <typeparamref name="THandler"/> as a packet handler.</summary>
    /// <typeparam name="THandler">Handler implementation.</typeparam>
    /// <returns><see langword="this"/> for chaining.</returns>
    IWireFishBuilder AddPacketHandler<THandler>() where THandler : class, IPacketHandler;
}
