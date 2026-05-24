using System.Net.Http.Headers;
using System.Text;
using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.Options;

namespace Novolis.Transports.Http.Authentication.Basic;

/// <summary>Applies HTTP Basic authentication from <see cref="BasicAuthenticationConfiguration"/>.</summary>
/// <param name="options">Configured credentials.</param>
public class BasicAuthentication(IOptions<BasicAuthenticationConfiguration> options) : IHttpAuthentication
{
    /// <inheritdoc />
    public Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Value.Username}:{options.Value.Password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        return Task.CompletedTask;
    }
}
