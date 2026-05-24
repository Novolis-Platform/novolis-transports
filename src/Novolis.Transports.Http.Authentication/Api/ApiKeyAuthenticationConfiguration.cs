namespace Novolis.Transports.Http.Authentication.Api;

/// <summary>API key header configuration.</summary>
public class ApiKeyAuthenticationConfiguration
{
    /// <summary>Header name for the API key (default <c>X-Api-Key</c>).</summary>
    public string HeaderName { get; set; } = "X-Api-Key";

    /// <summary>API key value sent on each request.</summary>
    public required string ApiKey { get; set; }
}
