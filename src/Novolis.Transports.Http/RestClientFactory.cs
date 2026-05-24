using Novolis.Transports.Http.Abstractions;

namespace Novolis.Transports.Http;

/// <inheritdoc cref="IRestClientFactory"/>
public class RestClientFactory(IHttpClientFactory httpClientFactory, IEnumerable<IRequestEnricher> enrichers, IEnumerable<IHttpAuthentication> authentications) : IRestClientFactory
{
    /// <inheritdoc />
    public IRestClient CreateClient(bool vanilla = false) =>
        vanilla
            ? new RestClient(httpClientFactory.CreateClient(), [], [])
            : new RestClient(httpClientFactory.CreateClient(), authentications, enrichers);

    /// <inheritdoc />
    public IRestClient CreateClient(IEnumerable<IRequestEnricher> enricherCollection, IEnumerable<IHttpAuthentication> authenticationCollection) =>
        new RestClient(httpClientFactory.CreateClient(), authenticationCollection, enricherCollection);
}
