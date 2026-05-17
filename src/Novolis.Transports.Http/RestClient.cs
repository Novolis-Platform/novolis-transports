using Novolis.Transports.Http.Abstractions;

namespace Novolis.Transports.Http;

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