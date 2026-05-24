using System.Net.Http.Headers;
using Novolis.Transports.Http.Abstractions;

namespace Novolis.Transports.Http.Authentication.Oidc;

/// <summary>Applies Bearer authentication using tokens from <see cref="IOidcTokenProvider"/>.</summary>
public class OidcAuthentication : IHttpAuthentication
{
    private readonly IOidcTokenProvider _tokenProvider;

    /// <summary>Creates an OIDC authenticator.</summary>
    /// <param name="tokenProvider">Token provider.</param>
    public OidcAuthentication(IOidcTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    /// <inheritdoc />
    public async Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
