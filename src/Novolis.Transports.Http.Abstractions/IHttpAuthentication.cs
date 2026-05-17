namespace Novolis.Transports.Http.Abstractions;

public interface IHttpAuthentication
{
    Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}