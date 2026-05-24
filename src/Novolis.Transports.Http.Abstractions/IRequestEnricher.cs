namespace Novolis.Transports.Http.Abstractions;

/// <summary>Adds headers or other metadata to outgoing HTTP requests before send.</summary>
public interface IRequestEnricher
{
    /// <summary>Enriches <paramref name="request"/> (for example correlation id, user agent).</summary>
    /// <param name="request">Outgoing request.</param>
    void Enrich(HttpRequestMessage request);
}
