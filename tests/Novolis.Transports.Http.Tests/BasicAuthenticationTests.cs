using System.Text;
using Novolis.Transports.Http.Authentication.Basic;
using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.Options;
using TUnit.Core;

namespace Novolis.Transports.Http.Tests;

public class BasicAuthenticationTests
{
    [Test]
    public async Task AuthenticateAsync_SetsBasicAuthorizationHeader()
    {
        var options = Options.Create(new BasicAuthenticationConfiguration
        {
            Username = "user",
            Password = "secret"
        });
        IHttpAuthentication sut = new BasicAuthentication(options);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/");

        await sut.AuthenticateAsync(request, CancellationToken.None);

        await Assert.That(request.Headers.Authorization).IsNotNull();
        await Assert.That(request.Headers.Authorization!.Scheme).IsEqualTo("Basic");
        var expected = Convert.ToBase64String(Encoding.ASCII.GetBytes("user:secret"));
        await Assert.That(request.Headers.Authorization.Parameter).IsEqualTo(expected);
    }

    [Test]
    public async Task AuthenticateAsync_WithSpecialCharacters_EncodesCredentials()
    {
        var options = Options.Create(new BasicAuthenticationConfiguration
        {
            Username = "a:b",
            Password = "c@d"
        });
        IHttpAuthentication sut = new BasicAuthentication(options);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/");

        await sut.AuthenticateAsync(request, CancellationToken.None);

        var decoded = Encoding.ASCII.GetString(Convert.FromBase64String(request.Headers.Authorization!.Parameter!));
        await Assert.That(decoded).IsEqualTo("a:b:c@d");
    }
}
