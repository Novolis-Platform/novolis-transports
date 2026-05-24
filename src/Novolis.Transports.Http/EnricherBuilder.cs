using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Http;

/// <inheritdoc cref="IEnricherBuilder"/>
public class EnricherBuilder : IEnricherBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>Creates a builder that registers into <paramref name="services"/>.</summary>
    /// <param name="services">Service collection.</param>
    public EnricherBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <inheritdoc />
    public IEnricherBuilder AddEnricher<T>() where T : class, IRequestEnricher
    {
        _services.AddSingleton<T>();
        _services.AddSingleton<IRequestEnricher>(provider => provider.GetRequiredService<T>());
        return this;
    }

    /// <inheritdoc />
    public IEnricherBuilder AddEnricher<T>(T enricher) where T : class, IRequestEnricher
    {
        _services.AddSingleton<T>(enricher);
        _services.AddSingleton<IRequestEnricher>(enricher);
        return this;
    }
}
