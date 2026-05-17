namespace Novolis.Transports.Http.Abstractions;

public interface IEnricherBuilder
{
    IEnricherBuilder AddEnricher<T>() where T : class, IRequestEnricher;
    IEnricherBuilder AddEnricher<T>(T enricher) where T : class, IRequestEnricher;
}