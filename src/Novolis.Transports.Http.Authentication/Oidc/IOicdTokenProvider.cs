namespace Novolis.Transports.Http.Authentication.Oidc;

public interface IOicdTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}