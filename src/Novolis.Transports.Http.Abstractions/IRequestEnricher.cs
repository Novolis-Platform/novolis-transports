namespace Novolis.Transports.Http.Abstractions;

public interface IRequestEnricher
{
    void Enrich(HttpRequestMessage request);
}