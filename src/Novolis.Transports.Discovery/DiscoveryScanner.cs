using System.Net;
using Novolis.Transports.Udp;

namespace Novolis.Transports.Discovery;

/// <summary>Sends one broadcast probe and yields matching discovery beacons.</summary>
public sealed class DiscoveryScanner
{
    /// <summary>Scans a UDP broadcast endpoint for a bounded period.</summary>
    public async IAsyncEnumerable<DiscoveryBeacon> ScanAsync(
        IPEndPoint endpoint,
        string probeToken,
        TimeSpan timeout,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);

        await using var client = new UdpDatagramChannel(
            new IPEndPoint(IPAddress.Any, 0),
            new UdpDatagramChannelOptions
            {
                EnableBroadcast = true,
            });
        await client.SendAsync(
            DiscoveryCodec.EncodeProbe(probeToken),
            endpoint,
            cancellationToken).ConfigureAwait(false);

        using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(timeout);
        while (!timeoutCancellation.IsCancellationRequested)
        {
            DiscoveryBeacon? beacon;
            try
            {
                var result = await client.ReceiveAsync(timeoutCancellation.Token)
                    .ConfigureAwait(false);
                beacon = DiscoveryCodec.TryDecodeBeacon(result.Payload);
            }
            catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested)
            {
                yield break;
            }

            if (beacon is not null)
                yield return beacon;
        }
    }
}
