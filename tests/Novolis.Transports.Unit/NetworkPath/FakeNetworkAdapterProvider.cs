using Novolis.Transports.NetworkPath;

namespace Novolis.Transports.Unit.NetworkPath;

internal sealed class FakeNetworkAdapterProvider(
    IReadOnlyList<NetworkAdapterInfo> adapters) : INetworkAdapterProvider
{
    public IReadOnlyList<NetworkAdapterInfo> GetAdapters() => adapters;
}
