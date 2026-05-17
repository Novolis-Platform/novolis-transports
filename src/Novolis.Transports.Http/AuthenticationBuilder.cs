using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Http;

public class AuthenticationBuilder : IAuthenticationBuilder
{
    private readonly IServiceCollection _services;

    public AuthenticationBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public IAuthenticationBuilder AddAuthentication<T>() where T : class, IHttpAuthentication
    {
        _services.AddSingleton<T>();
        _services.AddSingleton<IHttpAuthentication>(provider => provider.GetRequiredService<T>());
        return this;
    }

    public IAuthenticationBuilder AddAuthentication<T>(T authentication) where T : class, IHttpAuthentication
    {
        _services.AddSingleton<T>(authentication);
        _services.AddSingleton<IHttpAuthentication>(provider => provider.GetRequiredService<T>());
        return this;
    }
}