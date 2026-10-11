using System.Net;
using System.Net.Sockets;

namespace Novolis.Transports.NetworkPath;

/// <summary>Resolves the local source address with a connected UDP socket.</summary>
public sealed class SocketNetworkRouteResolver : INetworkRouteResolver
{
    public async Task<NetworkRoute> ResolveAsync(
        NetworkProbeTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(target);

        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(target.Host, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is SocketException or ArgumentException)
        {
            return new NetworkRoute(target, null, null, exception.Message);
        }

        var failures = new List<string>();
        foreach (var address in addresses)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var socket = new Socket(
                    address.AddressFamily,
                    SocketType.Dgram,
                    ProtocolType.Udp);
                socket.Connect(new IPEndPoint(address, target.Port));
                if (socket.LocalEndPoint is IPEndPoint local)
                    return new NetworkRoute(target, local.Address, null, null);

                failures.Add($"{address}: no local endpoint");
            }
            catch (SocketException exception)
            {
                failures.Add($"{address}: {exception.Message}");
            }
            catch (ArgumentException exception)
            {
                failures.Add($"{address}: {exception.Message}");
            }
        }

        var error = failures.Count == 0
            ? $"No address was resolved for {target.Host}."
            : string.Join("; ", failures);
        return new NetworkRoute(target, null, null, error);
    }
}
