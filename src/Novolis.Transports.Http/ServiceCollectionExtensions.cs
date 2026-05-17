using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Http;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNovolisHttpAuthentication<T>(this IServiceCollection services) where T : class, IHttpAuthentication
    {
        services.AddSingleton<IHttpAuthentication, T>();
        services.AddSingleton<IAuthenticationBuilder>(provider => provider.GetRequiredService<IAuthenticationBuilder>().AddAuthentication<T>());
        return services;
    }

    public static IServiceCollection AddNovolisHttpRequestEnricher<T>(this IServiceCollection services) where T : class, IRequestEnricher
    {
        services.AddSingleton<IRequestEnricher, T>();
        services.AddSingleton<IEnricherBuilder>(provider => provider.GetRequiredService<IEnricherBuilder>().AddEnricher<T>());
        return services;
    }

    public static IServiceCollection AddNovolisHttp(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<IRestClientFactory, RestClientFactory>();
        services.AddTransient<IRestClient>(provider => provider.GetRequiredService<IRestClientFactory>().CreateClient());
        return services;
    }

    public static IServiceCollection AddNovolisHttp(this IServiceCollection services, Action<IEnricherBuilder> configureEnrichers)
    {
        services.AddHttpClient();
        services.AddSingleton<IRestClientFactory, RestClientFactory>();
        services.AddTransient<IRestClient>(provider => provider.GetRequiredService<IRestClientFactory>().CreateClient());
        var enricherBuilder = new EnricherBuilder(services);
        configureEnrichers(enricherBuilder);
        return services;
    }

    public static IServiceCollection AddNovolisHttp(this IServiceCollection services, Action<IAuthenticationBuilder> configureAuthentications)
    {
        services.AddHttpClient();
        services.AddSingleton<IRestClientFactory, RestClientFactory>();
        services.AddTransient<IRestClient>(provider => provider.GetRequiredService<IRestClientFactory>().CreateClient());
        var authenticationBuilder = new AuthenticationBuilder(services);
        configureAuthentications(authenticationBuilder);
        return services;
    }

    public static IServiceCollection AddNovolisHttp(this IServiceCollection services, Action<IEnricherBuilder> configureEnrichers, Action<IAuthenticationBuilder> configureAuthentications)
    {
        services.AddHttpClient();
        services.AddSingleton<IRestClientFactory, RestClientFactory>();
        services.AddTransient<IRestClient>(provider => provider.GetRequiredService<IRestClientFactory>().CreateClient());
        var enricherBuilder = new EnricherBuilder(services);
        configureEnrichers(enricherBuilder);
        var authenticationBuilder = new AuthenticationBuilder(services);
        configureAuthentications(authenticationBuilder);
        return services;
    }

    [Obsolete("Use AddNovolisHttpAuthentication. This Frank-prefixed name will be removed in a future release.")]
    public static IServiceCollection AddFrankHttpAuthentication<T>(this IServiceCollection services) where T : class, IHttpAuthentication
        => services.AddNovolisHttpAuthentication<T>();

    [Obsolete("Use AddNovolisHttpRequestEnricher. This Frank-prefixed name will be removed in a future release.")]
    public static IServiceCollection AddFrankHttpRequestEnricher<T>(this IServiceCollection services) where T : class, IRequestEnricher
        => services.AddNovolisHttpRequestEnricher<T>();

    [Obsolete("Use AddNovolisHttp. This Frank-prefixed name will be removed in a future release.")]
    public static IServiceCollection AddFrankHttp(this IServiceCollection services)
        => services.AddNovolisHttp();

    [Obsolete("Use AddNovolisHttp. This Frank-prefixed name will be removed in a future release.")]
    public static IServiceCollection AddFrankHttp(this IServiceCollection services, Action<IEnricherBuilder> configureEnrichers)
        => services.AddNovolisHttp(configureEnrichers);

    [Obsolete("Use AddNovolisHttp. This Frank-prefixed name will be removed in a future release.")]
    public static IServiceCollection AddFrankHttp(this IServiceCollection services, Action<IAuthenticationBuilder> configureAuthentications)
        => services.AddNovolisHttp(configureAuthentications);

    [Obsolete("Use AddNovolisHttp. This Frank-prefixed name will be removed in a future release.")]
    public static IServiceCollection AddFrankHttp(this IServiceCollection services, Action<IEnricherBuilder> configureEnrichers, Action<IAuthenticationBuilder> configureAuthentications)
        => services.AddNovolisHttp(configureEnrichers, configureAuthentications);
}
