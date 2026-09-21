using System.Net;
using System.Net.Sockets;

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

        using var client = new UdpClient
        {
            EnableBroadcast = true,
        };
        await client.SendAsync(
            DiscoveryCodec.EncodeProbe(probeToken),
            endpoint,
            cancellationToken).ConfigureAwait(false);

        using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(timeout);
        while (!timeoutCancellation.IsCancellationRequested)
        {
            UdpReceiveResult result;
            try
            {
                result = await client.ReceiveAsync(timeoutCancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested)
            {
                yield break;
            }

            var beacon = DiscoveryCodec.TryDecodeBeacon(result.Buffer);
            if (beacon is not null)
                yield return beacon;
        }
    }
}
