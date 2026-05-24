namespace Novolis.Transports.Http.Authentication.Oidc;

/// <summary>Client credentials settings for OIDC token acquisition.</summary>
public class OidcAuthenticationConfiguration
{
    /// <summary>OAuth client id.</summary>
    public required string ClientId { get; set; }

    /// <summary>OAuth client secret.</summary>
    public required string ClientSecret { get; set; }

    /// <summary>Requested scope.</summary>
    public required string Scope { get; set; }

    /// <summary>Token endpoint path or URL (default <c>connect/token</c>).</summary>
    public string? TokenEndpoint { get; set; } = "connect/token";
}
