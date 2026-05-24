using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Http;

/// <inheritdoc cref="IAuthenticationBuilder"/>
public class AuthenticationBuilder : IAuthenticationBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>Creates a builder that registers into <paramref name="services"/>.</summary>
    /// <param name="services">Service collection.</param>
    public AuthenticationBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <inheritdoc />
    public IAuthenticationBuilder AddAuthentication<T>() where T : class, IHttpAuthentication
    {
        _services.AddSingleton<T>();
        _services.AddSingleton<IHttpAuthentication>(provider => provider.GetRequiredService<T>());
        return this;
    }

    /// <inheritdoc />
    public IAuthenticationBuilder AddAuthentication<T>(T authentication) where T : class, IHttpAuthentication
    {
        _services.AddSingleton<T>(authentication);
        _services.AddSingleton<IHttpAuthentication>(provider => provider.GetRequiredService<T>());
        return this;
    }
}
