using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.Options;

namespace Novolis.Transports.Http.Authentication.Api;

/// <summary>Adds a configured API key header to outgoing requests.</summary>
public class ApiKeyAuthentication : IHttpAuthentication
{
    private readonly IOptions<ApiKeyAuthenticationConfiguration> _options;

    /// <summary>Creates an authenticator using <paramref name="options"/>.</summary>
    /// <param name="options">API key configuration.</param>
    public ApiKeyAuthentication(IOptions<ApiKeyAuthenticationConfiguration> options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add(_options.Value.HeaderName, _options.Value.ApiKey);
        return Task.CompletedTask;
    }
}
