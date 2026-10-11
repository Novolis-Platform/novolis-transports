namespace Novolis.Transports.NetworkPath;

/// <summary>Supplies a snapshot of local network adapters.</summary>
public interface INetworkAdapterProvider
{
    IReadOnlyList<NetworkAdapterInfo> GetAdapters();
}
