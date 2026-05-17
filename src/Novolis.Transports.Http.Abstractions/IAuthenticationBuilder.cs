namespace Novolis.Transports.Http.Abstractions;

public interface IAuthenticationBuilder
{
    IAuthenticationBuilder AddAuthentication<T>() where T : class, IHttpAuthentication;
    IAuthenticationBuilder AddAuthentication<T>(T authentication) where T : class, IHttpAuthentication;
}