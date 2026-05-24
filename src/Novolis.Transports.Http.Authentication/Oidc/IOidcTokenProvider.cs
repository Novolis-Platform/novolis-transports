namespace Novolis.Transports.Http.Authentication.Oidc;

/// <summary>Provides OIDC access tokens (typically client credentials flow).</summary>
public interface IOidcTokenProvider
{
    /// <summary>Obtains a bearer access token, using cache when configured.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Access token string.</returns>
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}
