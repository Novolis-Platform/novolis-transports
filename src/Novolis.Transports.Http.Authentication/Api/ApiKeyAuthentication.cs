using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.Options;

namespace Novolis.Transports.Http.Authentication.Api;

public class ApiKeyAuthentication : IHttpAuthentication
{
    private readonly IOptions<ApiKeyAuthenticationConfiguration> _options;

    public ApiKeyAuthentication(IOptions<ApiKeyAuthenticationConfiguration> options)
    {
        _options = options;
    }
    
    public Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add(_options.Value.HeaderName, _options.Value.ApiKey);
        return Task.CompletedTask;
    }
}