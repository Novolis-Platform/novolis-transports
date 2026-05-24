namespace Novolis.Transports.Http.Authentication.Basic;

/// <summary>Username and password for HTTP Basic authentication.</summary>
public class BasicAuthenticationConfiguration
{
    /// <summary>Basic auth username.</summary>
    public required string Username { get; set; }

    /// <summary>Basic auth password.</summary>
    public required string Password { get; set; }
}
