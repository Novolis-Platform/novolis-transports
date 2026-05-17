namespace Novolis.Transports.Http.Authentication.Oidc;

public interface IOidcTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}
