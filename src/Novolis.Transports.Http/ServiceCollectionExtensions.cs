using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Http;

/// <summary>Dependency injection extensions for Novolis HTTP clients.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <typeparamref name="T"/> as the default <see cref="IHttpAuthentication"/>.</summary>
    /// <typeparam name="T">Authentication handler type.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddNovolisHttpAuthentication<T>(this IServiceCollection services) where T : class, IHttpAuthentication
    {
        services.AddSingleton<IHttpAuthentication, T>();
        services.AddSingleton<IAuthenticationBuilder>(provider => provider.GetRequiredService<IAuthenticationBuilder>().AddAuthentication<T>());
        return services;
    }

    /// <summary>Registers <typeparamref name="T"/> as a request enricher.</summary>
    /// <typeparam name="T">Enricher type.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddNovolisHttpRequestEnricher<T>(this IServiceCollection services) where T : class, IRequestEnricher
    {
        services.AddSingleton<IRequestEnricher, T>();
        services.AddSingleton<IEnricherBuilder>(provider => provider.GetRequiredService<IEnricherBuilder>().AddEnricher<T>());
        return services;
    }

    /// <summary>Registers <see cref="IRestClient"/>, <see cref="IRestClientFactory"/>, and <see cref="HttpClient"/> factory.</summary>
    /// <param name="services">Service collection.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddNovolisHttp(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<IRestClientFactory, RestClientFactory>();
        services.AddTransient<IRestClient>(provider => provider.GetRequiredService<IRestClientFactory>().CreateClient());
        return services;
    }

    /// <summary>Registers Novolis HTTP services and configures request enrichers.</summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configureEnrichers">Enricher registration callback.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddNovolisHttp(this IServiceCollection services, Action<IEnricherBuilder> configureEnrichers)
    {
        services.AddHttpClient();
        services.AddSingleton<IRestClientFactory, RestClientFactory>();
        services.AddTransient<IRestClient>(provider => provider.GetRequiredService<IRestClientFactory>().CreateClient());
        var enricherBuilder = new EnricherBuilder(services);
        configureEnrichers(enricherBuilder);
        return services;
    }

    /// <summary>Registers Novolis HTTP services and configures authentication handlers.</summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configureAuthentications">Authentication registration callback.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddNovolisHttp(this IServiceCollection services, Action<IAuthenticationBuilder> configureAuthentications)
    {
        services.AddHttpClient();
        services.AddSingleton<IRestClientFactory, RestClientFactory>();
        services.AddTransient<IRestClient>(provider => provider.GetRequiredService<IRestClientFactory>().CreateClient());
        var authenticationBuilder = new AuthenticationBuilder(services);
        configureAuthentications(authenticationBuilder);
        return services;
    }

    /// <summary>Registers Novolis HTTP services and configures enrichers and authentications.</summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configureEnrichers">Enricher registration callback.</param>
    /// <param name="configureAuthentications">Authentication registration callback.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
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

    /// <inheritdoc cref="AddNovolisHttpAuthentication{T}"/>
    [Obsolete("Use AddNovolisHttpAuthentication. This Frank-prefixed name will be removed in a future release.")]
    public static IServiceCollection AddFrankHttpAuthentication<T>(this IServiceCollection services) where T : class, IHttpAuthentication
        => services.AddNovolisHttpAuthentication<T>();

    /// <inheritdoc cref="AddNovolisHttpRequestEnricher{T}"/>
    [Obsolete("Use AddNovolisHttpRequestEnricher. This Frank-prefixed name will be removed in a future release.")]
    public static IServiceCollection AddFrankHttpRequestEnricher<T>(this IServiceCollection services) where T : class, IRequestEnricher
        => services.AddNovolisHttpRequestEnricher<T>();

    /// <inheritdoc cref="AddNovolisHttp(IServiceCollection)"/>
    [Obsolete("Use AddNovolisHttp. This Frank-prefixed name will be removed in a future release.")]
    public static IServiceCollection AddFrankHttp(this IServiceCollection services)
        => services.AddNovolisHttp();

    /// <inheritdoc cref="AddNovolisHttp(IServiceCollection, Action{IEnricherBuilder})"/>
    [Obsolete("Use AddNovolisHttp. This Frank-prefixed name will be removed in a future release.")]
    public static IServiceCollection AddFrankHttp(this IServiceCollection services, Action<IEnricherBuilder> configureEnrichers)
        => services.AddNovolisHttp(configureEnrichers);

    /// <inheritdoc cref="AddNovolisHttp(IServiceCollection, Action{IAuthenticationBuilder})"/>
    [Obsolete("Use AddNovolisHttp. This Frank-prefixed name will be removed in a future release.")]
    public static IServiceCollection AddFrankHttp(this IServiceCollection services, Action<IAuthenticationBuilder> configureAuthentications)
        => services.AddNovolisHttp(configureAuthentications);

    /// <inheritdoc cref="AddNovolisHttp(IServiceCollection, Action{IEnricherBuilder}, Action{IAuthenticationBuilder})"/>
    [Obsolete("Use AddNovolisHttp. This Frank-prefixed name will be removed in a future release.")]
    public static IServiceCollection AddFrankHttp(this IServiceCollection services, Action<IEnricherBuilder> configureEnrichers, Action<IAuthenticationBuilder> configureAuthentications)
        => services.AddNovolisHttp(configureEnrichers, configureAuthentications);
}
