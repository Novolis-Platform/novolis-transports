using System.Net;
using Novolis.Transports.Udp;

namespace Novolis.Transports.Discovery;

/// <summary>Answers generic UDP discovery probes with a fixed beacon.</summary>
public sealed class DiscoveryResponder : IAsyncDisposable
{
    private readonly UdpDatagramChannel _client;
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
        _client = new UdpDatagramChannel(endpoint);
        _probeToken = probeToken;
        _beacon = DiscoveryCodec.EncodeBeacon(beacon);
    }

    /// <summary>Runs until cancellation.</summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _client.ReceiveAsync(cancellationToken)
                    .ConfigureAwait(false);
                var probe = System.Text.Encoding.UTF8.GetString(result.Payload);
                if (string.Equals(probe, _probeToken, StringComparison.Ordinal))
                {
                    await _client.SendAsync(
                            _beacon,
                            (IPEndPoint)result.RemoteEndpoint,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => _client.DisposeAsync();
}
