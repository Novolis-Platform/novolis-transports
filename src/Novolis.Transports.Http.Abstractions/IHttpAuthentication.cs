namespace Novolis.Transports.Http.Abstractions;

/// <summary>Applies authentication headers or credentials to outgoing HTTP requests.</summary>
public interface IHttpAuthentication
{
    /// <summary>Mutates <paramref name="request"/> with authentication (for example Bearer or Basic).</summary>
    /// <param name="request">Outgoing request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when authentication is applied.</returns>
    Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}
