using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace Novolis.Transports.Udp;

/// <summary>Asynchronous UDP channel with a bounded receive queue.</summary>
public sealed class UdpDatagramChannel : ITransportDatagramChannel
{
    private readonly UdpClient _client;
    private readonly UdpDatagramChannelOptions _options;
    private readonly Channel<TransportDatagram> _received;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly Task _receiveTask;
    private long _sentPackets;
    private long _sentBytes;
    private long _receivedPackets;
    private long _receivedBytes;
    private long _droppedPackets;
    private long _rejectedPackets;
    private int _disposed;

    /// <summary>Binds a channel to a local endpoint.</summary>
    public UdpDatagramChannel(
        IPEndPoint localEndpoint,
        UdpDatagramChannelOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(localEndpoint);
        _options = options ?? new UdpDatagramChannelOptions();
        ValidateOptions(_options);
        _client = new UdpClient(localEndpoint)
        {
            EnableBroadcast = _options.EnableBroadcast,
        };
        LocalEndPoint = (IPEndPoint)_client.Client.LocalEndPoint!;
        _received = Channel.CreateBounded<TransportDatagram>(
            new BoundedChannelOptions(_options.ReceiveQueueCapacity)
            {
                FullMode = _options.ReceiveQueueFullMode,
                SingleReader = false,
                SingleWriter = true,
            });
        _receiveTask = ReceiveLoopAsync(_lifetime.Token);
    }

    /// <inheritdoc />
    public int MaximumPayloadSize => _options.MaximumPayloadSize;

    /// <inheritdoc />
    public IPEndPoint LocalEndPoint { get; }

    /// <summary>Gets a snapshot of channel activity.</summary>
    public TransportChannelStatistics Statistics => new(
        Interlocked.Read(ref _sentPackets),
        Interlocked.Read(ref _sentBytes),
        Interlocked.Read(ref _receivedPackets),
        Interlocked.Read(ref _receivedBytes),
        Interlocked.Read(ref _droppedPackets),
        Interlocked.Read(ref _rejectedPackets));

    /// <inheritdoc />
    public async ValueTask<int> SendAsync(
        ReadOnlyMemory<byte> payload,
        IPEndPoint endpoint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        if (payload.Length > _options.MaximumPayloadSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(payload),
                payload.Length,
                $"UDP payloads cannot exceed {_options.MaximumPayloadSize} bytes.");
        }

        var sent = await _client.SendAsync(
                payload,
                endpoint,
                cancellationToken)
            .ConfigureAwait(false);
        Interlocked.Increment(ref _sentPackets);
        Interlocked.Add(ref _sentBytes, sent);
        return sent;
    }

    /// <inheritdoc />
    public async ValueTask<TransportDatagram> ReceiveAsync(
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        try
        {
            return await _received.Reader.ReadAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (ChannelClosedException) when (Volatile.Read(ref _disposed) != 0)
        {
            throw new ObjectDisposedException(nameof(UdpDatagramChannel));
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        _lifetime.Cancel();
        _client.Dispose();
        try
        {
            await _receiveTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
        }
        finally
        {
            _received.Writer.TryComplete();
            _lifetime.Dispose();
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult result;
                try
                {
                    result = await _client.ReceiveAsync(cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (SocketException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (result.Buffer.Length > _options.MaximumPayloadSize)
                {
                    Interlocked.Increment(ref _rejectedPackets);
                    continue;
                }

                var datagram = new TransportDatagram(
                    result.RemoteEndPoint,
                    result.Buffer,
                    DateTimeOffset.UtcNow);
                Interlocked.Increment(ref _receivedPackets);
                Interlocked.Add(ref _receivedBytes, result.Buffer.Length);
                if (!_received.Writer.TryWrite(datagram))
                    Interlocked.Increment(ref _droppedPackets);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            _received.Writer.TryComplete();
        }
    }

    private static void ValidateOptions(UdpDatagramChannelOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            options.MaximumPayloadSize,
            0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            options.ReceiveQueueCapacity,
            0);
    }

    private void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);
}
