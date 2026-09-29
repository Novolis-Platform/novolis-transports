using System.Net;

namespace Novolis.Transports;

/// <summary>Accepts established transport connections.</summary>
public interface ITransportListener : IAsyncDisposable
{
    /// <summary>Gets the endpoint selected by the listener.</summary>
    EndPoint LocalEndPoint { get; }

    /// <summary>Accepts the next connection.</summary>
    ValueTask<ITransportConnection> AcceptConnectionAsync(
        CancellationToken cancellationToken = default);
}
