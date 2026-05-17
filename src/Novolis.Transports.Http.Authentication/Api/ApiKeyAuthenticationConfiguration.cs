namespace Novolis.Transports.Http.Authentication.Api;

public class ApiKeyAuthenticationConfiguration
{
    /// <summary>
    /// The name of the header to add the API key to.
    /// </summary>
    /// <remarks> Default is "X-Api-Key". </remarks>
    public string HeaderName { get; set; } = "X-Api-Key";
    
    /// <summary>
    /// The API key to add to the request.
    /// </summary>
    /// <remarks> Required. </remarks>
    public string ApiKey { get; set; } = null!;
}