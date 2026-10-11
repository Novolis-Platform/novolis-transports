using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Novolis.Transports.NetworkPath;

/// <summary>Measures TCP connection latency through a selected local address.</summary>
public sealed class TcpNetworkPathProbe : INetworkPathProbe
{
    public async Task<NetworkProbeSample> ProbeAsync(
        NetworkAdapterInfo adapter,
        NetworkProbeTarget target,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(adapter);
        ArgumentNullException.ThrowIfNull(target);
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        var sources = adapter.Addresses.ToArray();
        IPAddress[] destinations;
        try
        {
            destinations = await ResolveDestinationsAsync(target, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is SocketException or ArgumentException)
        {
            return new NetworkProbeSample(
                null,
                false,
                sources.FirstOrDefault(),
                exception.Message);
        }

        var stopwatch = Stopwatch.StartNew();
        string? lastError = null;

        using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(timeout);
        foreach (var destination in destinations)
        {
            var source = sources.FirstOrDefault(address =>
                address.AddressFamily == destination.AddressFamily);
            if (source is null)
                continue;

            try
            {
                using var socket = new Socket(
                    destination.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp)
                {
                    NoDelay = true
                };
                socket.Bind(new IPEndPoint(source, 0));
                await socket.ConnectAsync(
                        new IPEndPoint(destination, target.Port),
                        timeoutCancellation.Token)
                    .ConfigureAwait(false);
                stopwatch.Stop();
                return new NetworkProbeSample(
                    stopwatch.Elapsed,
                    true,
                    source,
                    null);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                return new NetworkProbeSample(
                    null,
                    false,
                    source,
                    $"Timed out after {timeout.TotalMilliseconds:0} ms.");
            }
            catch (Exception exception) when (
                exception is SocketException or IOException or ArgumentException)
            {
                lastError = exception.Message;
            }
        }

        stopwatch.Stop();
        var error = lastError ?? $"No address on {adapter.Name} can reach {target}.";
        return new NetworkProbeSample(
            null,
            false,
            sources.FirstOrDefault(),
            error);
    }

    private static async Task<IPAddress[]> ResolveDestinationsAsync(
        NetworkProbeTarget target,
        CancellationToken cancellationToken) =>
        await Dns.GetHostAddressesAsync(target.Host, cancellationToken)
            .ConfigureAwait(false);
}
