namespace Novolis.Transports.Http.Abstractions;

public interface IRestClientFactory
{
    /// <summary>
    /// Create a new instance of <see cref="IRestClient"/>
    /// </summary>
    /// <param name="vanilla">Vanilla mode will not use any enrichers or authentications</param>
    /// <returns>A new instance of <see cref="IRestClient"/></returns>
    IRestClient CreateClient(bool vanilla = false);
    
    /// <summary>
    /// Create a new instance of <see cref="IRestClient"/>
    /// </summary>
    /// <param name="enricherCollection"></param>
    /// <param name="authentications"></param>
    /// <returns>A new instance of <see cref="IRestClient"/></returns>
    IRestClient CreateClient(IEnumerable<IRequestEnricher> enricherCollection, IEnumerable<IHttpAuthentication> authentications);
}