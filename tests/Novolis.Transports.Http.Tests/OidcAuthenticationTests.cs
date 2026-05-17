using Novolis.Transports.Http.Authentication.Oidc;
using Novolis.Transports.Http.Abstractions;
using TUnit.Core;

namespace Novolis.Transports.Http.Tests;

public class OidcAuthenticationTests
{
    [Test]
    public async Task AuthenticateAsync_SetsBearerTokenFromProvider()
    {
        IHttpAuthentication sut = new OidcAuthentication(new FixedTokenProvider("my.jwt.token"));
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/");

        await sut.AuthenticateAsync(request, CancellationToken.None);

        await Assert.That(request.Headers.Authorization).IsNotNull();
        await Assert.That(request.Headers.Authorization!.Scheme).IsEqualTo("Bearer");
        await Assert.That(request.Headers.Authorization.Parameter).IsEqualTo("my.jwt.token");
    }

    private sealed class FixedTokenProvider(string token) : IOidcTokenProvider
    {
        public Task<string> GetTokenAsync(CancellationToken cancellationToken) => Task.FromResult(token);
    }
}
