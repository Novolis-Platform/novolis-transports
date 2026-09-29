namespace Novolis.Transports;

/// <summary>Represents an established transport connection.</summary>
public interface ITransportConnection : IAsyncDisposable
{
    /// <summary>Gets connection identity, endpoints, and capabilities.</summary>
    TransportConnectionInfo Info { get; }

    /// <summary>Opens a locally initiated bidirectional stream.</summary>
    ValueTask<ITransportStream> OpenBidirectionalStreamAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Accepts a remotely initiated stream.</summary>
    ValueTask<ITransportStream> AcceptInboundStreamAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Closes the connection with an application error code.</summary>
    ValueTask CloseAsync(
        long errorCode = 0,
        CancellationToken cancellationToken = default);
}
