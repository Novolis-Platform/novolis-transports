namespace Novolis.Transports.Http.Abstractions;

public interface IRestClient
{
    /// <summary>
    /// Send a request to the server
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}