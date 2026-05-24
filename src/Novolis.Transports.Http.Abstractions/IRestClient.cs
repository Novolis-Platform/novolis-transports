namespace Novolis.Transports.Http.Abstractions;

/// <summary>HTTP client that applies registered authentications and enrichers before send.</summary>
public interface IRestClient
{
    /// <summary>Sends an HTTP request after applying authentications and enrichers.</summary>
    /// <param name="request">Request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}
