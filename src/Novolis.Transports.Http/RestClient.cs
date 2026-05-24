using Novolis.Transports.Http.Abstractions;

namespace Novolis.Transports.Http;

/// <summary>
/// Default <see cref="IRestClient"/> that applies all registered <see cref="IHttpAuthentication"/>
/// and <see cref="IRequestEnricher"/> instances before delegating to an <see cref="HttpClient"/>.
/// </summary>
public class RestClient(HttpClient httpClient, IEnumerable<IHttpAuthentication> authentications, IEnumerable<IRequestEnricher> enrichers) : IRestClient
{
    /// <inheritdoc />
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        foreach (var authentication in authentications)
        {
            await authentication.AuthenticateAsync(request, cancellationToken);
        }

        foreach (var enricher in enrichers)
        {
            enricher.Enrich(request);
        }

        return await httpClient.SendAsync(request, cancellationToken);
    }
}
