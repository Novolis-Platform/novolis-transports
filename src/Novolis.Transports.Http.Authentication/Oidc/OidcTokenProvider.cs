using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Novolis.Transports.Http.Authentication.Oidc;

/// <summary>Client-credentials OIDC token provider with in-memory caching.</summary>
public class OidcTokenProvider : IOidcTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly IOptions<OidcAuthenticationConfiguration> _options;

    private string _tokenKey => "oidc-token";

    /// <summary>Creates a token provider.</summary>
    /// <param name="httpClient">HTTP client for the token endpoint.</param>
    /// <param name="options">OIDC configuration.</param>
    /// <param name="cache">Memory cache for tokens.</param>
    public OidcTokenProvider(HttpClient httpClient, IOptions<OidcAuthenticationConfiguration> options, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _options = options;
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<string>(_tokenKey, out var token) && token is not null)
        {
            return token;
        }

        var tokenEndpoint = _options.Value.TokenEndpoint
            ?? throw new InvalidOperationException($"{nameof(OidcAuthenticationConfiguration.TokenEndpoint)} is not configured.");

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.Value.ClientId,
            ["client_secret"] = _options.Value.ClientSecret,
            ["scope"] = _options.Value.Scope
        });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content)
            ?? throw new InvalidOperationException("Token endpoint returned an empty or invalid JSON body.");

        _cache.Set(_tokenKey, tokenResponse.AccessToken, TimeSpan.FromSeconds(300));

        return tokenResponse.AccessToken;
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken);
}
