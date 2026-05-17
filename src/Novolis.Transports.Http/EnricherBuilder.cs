using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Http;

public class EnricherBuilder : IEnricherBuilder
{
    private readonly IServiceCollection _services;

    public EnricherBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public IEnricherBuilder AddEnricher<T>() where T : class, IRequestEnricher
    {
        _services.AddSingleton<T>();
        _services.AddSingleton<IRequestEnricher>(provider => provider.GetRequiredService<T>());
        return this;
    }

    public IEnricherBuilder AddEnricher<T>(T enricher) where T : class, IRequestEnricher
    {
        _services.AddSingleton<T>(enricher);
        _services.AddSingleton<IRequestEnricher>(enricher);
        return this;
    }
}