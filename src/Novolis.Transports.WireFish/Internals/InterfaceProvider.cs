using System.Net.NetworkInformation;

namespace Novolis.Transports.WireFish.Internals;

internal class InterfaceProvider
{
    private readonly IEnumerable<NetworkInterface> _interfaces;
    
    public InterfaceProvider()
    {
        _interfaces = NetworkInterface.GetAllNetworkInterfaces();
    }
    
    public IEnumerable<NetworkInterface> GetInterfaces() => _interfaces;
}