namespace Novolis.Transports.Http.Abstractions;

/// <summary>Creates <see cref="IRestClient"/> instances with optional authentication and enricher sets.</summary>
public interface IRestClientFactory
{
    /// <summary>
    /// Creates a client using the default DI-registered enrichers and authentications, or a vanilla client.
    /// </summary>
    /// <param name="vanilla">When <see langword="true"/>, skips enrichers and authentications.</param>
    /// <returns>A new <see cref="IRestClient"/>.</returns>
    IRestClient CreateClient(bool vanilla = false);

    /// <summary>Creates a client with explicit enricher and authentication collections.</summary>
    /// <param name="enricherCollection">Request enrichers to apply.</param>
    /// <param name="authentications">Authentication handlers to apply.</param>
    /// <returns>A new <see cref="IRestClient"/>.</returns>
    IRestClient CreateClient(IEnumerable<IRequestEnricher> enricherCollection, IEnumerable<IHttpAuthentication> authentications);
}
