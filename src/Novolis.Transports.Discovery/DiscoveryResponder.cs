using System.Net;
using System.Net.Sockets;

namespace Novolis.Transports.Discovery;

/// <summary>Answers generic UDP discovery probes with a fixed beacon.</summary>
public sealed class DiscoveryResponder : IAsyncDisposable
{
    private readonly UdpClient _client;
    private readonly string _probeToken;
    private readonly byte[] _beacon;

    /// <summary>Creates a responder bound to the requested UDP endpoint.</summary>
    public DiscoveryResponder(
        IPEndPoint endpoint,
        string probeToken,
        DiscoveryBeacon beacon)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(probeToken);
        ArgumentNullException.ThrowIfNull(beacon);
        _client = new UdpClient(endpoint);
        _probeToken = probeToken;
        _beacon = DiscoveryCodec.EncodeBeacon(beacon);
    }

    /// <summary>Runs until cancellation.</summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            UdpReceiveResult result;
            try
            {
                result = await _client.ReceiveAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var probe = System.Text.Encoding.UTF8.GetString(result.Buffer);
            if (string.Equals(probe, _probeToken, StringComparison.Ordinal))
            {
                await _client.SendAsync(
                    _beacon,
                    result.RemoteEndPoint,
                    cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }
}
