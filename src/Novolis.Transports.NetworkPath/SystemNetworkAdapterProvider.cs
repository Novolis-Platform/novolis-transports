using System.Net;
using System.Net.NetworkInformation;

namespace Novolis.Transports.NetworkPath;

/// <summary>Reads local adapters through the cross-platform .NET network APIs.</summary>
public sealed class SystemNetworkAdapterProvider : INetworkAdapterProvider
{
    public IReadOnlyList<NetworkAdapterInfo> GetAdapters()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Select(ReadAdapter)
            .Where(adapter => adapter.Addresses.Count > 0 || adapter.Gateways.Count > 0)
            .OrderBy(adapter => adapter.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static NetworkAdapterInfo ReadAdapter(NetworkInterface networkInterface)
    {
        try
        {
            var properties = networkInterface.GetIPProperties();
            return new NetworkAdapterInfo(
                networkInterface.Id,
                networkInterface.Name,
                networkInterface.Description,
                networkInterface.NetworkInterfaceType,
                networkInterface.OperationalStatus,
                GetInterfaceIndex(properties),
                properties.UnicastAddresses
                    .Select(address => address.Address)
                    .ToArray(),
                properties.GatewayAddresses
                    .Select(gateway => gateway.Address)
                    .ToArray());
        }
        catch (NetworkInformationException)
        {
            return EmptyAdapter(networkInterface);
        }
        catch (InvalidOperationException)
        {
            return EmptyAdapter(networkInterface);
        }
    }

    private static NetworkAdapterInfo EmptyAdapter(NetworkInterface networkInterface) =>
        new(
            networkInterface.Id,
            networkInterface.Name,
            networkInterface.Description,
            networkInterface.NetworkInterfaceType,
            networkInterface.OperationalStatus,
            null,
            Array.Empty<IPAddress>(),
            Array.Empty<IPAddress>());

    private static int? GetInterfaceIndex(IPInterfaceProperties properties)
    {
        try
        {
            return properties.GetIPv4Properties().Index;
        }
        catch (NetworkInformationException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }

        try
        {
            return properties.GetIPv6Properties().Index;
        }
        catch (NetworkInformationException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }

        return null;
    }
}
