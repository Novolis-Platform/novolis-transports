namespace Novolis.Transports.Http.Authentication.Oidc;

public class OidcAuthenticationConfiguration
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string Scope { get; set; }
    public string? TokenEndpoint { get; set; } = "connect/token";
}