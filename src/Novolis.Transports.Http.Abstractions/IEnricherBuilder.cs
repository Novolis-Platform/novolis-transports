namespace Novolis.Transports.Http.Abstractions;

/// <summary>Fluent registration of HTTP request enrichers for DI.</summary>
public interface IEnricherBuilder
{
    /// <summary>Registers <typeparamref name="T"/> as a singleton request enricher.</summary>
    /// <typeparam name="T">Enricher implementation.</typeparam>
    /// <returns><see langword="this"/> for chaining.</returns>
    IEnricherBuilder AddEnricher<T>() where T : class, IRequestEnricher;

    /// <summary>Registers a specific enricher instance.</summary>
    /// <typeparam name="T">Enricher implementation type.</typeparam>
    /// <param name="enricher">Instance to register.</param>
    /// <returns><see langword="this"/> for chaining.</returns>
    IEnricherBuilder AddEnricher<T>(T enricher) where T : class, IRequestEnricher;
}
