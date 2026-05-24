namespace Novolis.Transports.Http.Abstractions;

/// <summary>Fluent registration of HTTP authentication handlers for DI.</summary>
public interface IAuthenticationBuilder
{
    /// <summary>Registers <typeparamref name="T"/> as a singleton authentication handler.</summary>
    /// <typeparam name="T">Authentication implementation.</typeparam>
    /// <returns><see langword="this"/> for chaining.</returns>
    IAuthenticationBuilder AddAuthentication<T>() where T : class, IHttpAuthentication;

    /// <summary>Registers a specific authentication instance.</summary>
    /// <typeparam name="T">Authentication implementation type.</typeparam>
    /// <param name="authentication">Instance to register.</param>
    /// <returns><see langword="this"/> for chaining.</returns>
    IAuthenticationBuilder AddAuthentication<T>(T authentication) where T : class, IHttpAuthentication;
}
