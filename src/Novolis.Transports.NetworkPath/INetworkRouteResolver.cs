namespace Novolis.Transports.NetworkPath;

/// <summary>Resolves the source address selected for a destination without sending payload data.</summary>
public interface INetworkRouteResolver
{
    Task<NetworkRoute> ResolveAsync(
        NetworkProbeTarget target,
        CancellationToken cancellationToken = default);
}
