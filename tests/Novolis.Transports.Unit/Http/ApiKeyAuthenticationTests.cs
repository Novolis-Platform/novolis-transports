using Novolis.Transports.Http.Authentication.Api;
using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.Options;
using TUnit.Core;

namespace Novolis.Transports.Http.Tests;

public class ApiKeyAuthenticationTests
{
    [Test]
    public async Task AuthenticateAsync_UsesDefaultHeaderName()
    {
        var options = Options.Create(new ApiKeyAuthenticationConfiguration { ApiKey = "k-123" });
        IHttpAuthentication sut = new ApiKeyAuthentication(options);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/");

        await sut.AuthenticateAsync(request, CancellationToken.None);

        await Assert.That(request.Headers.Contains("X-Api-Key")).IsTrue();
        await Assert.That(request.Headers.GetValues("X-Api-Key").Single()).IsEqualTo("k-123");
    }

    [Test]
    public async Task AuthenticateAsync_UsesCustomHeaderName()
    {
        var options = Options.Create(new ApiKeyAuthenticationConfiguration
        {
            HeaderName = "Authorization",
            ApiKey = "raw-token"
        });
        IHttpAuthentication sut = new ApiKeyAuthentication(options);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/");

        await sut.AuthenticateAsync(request, CancellationToken.None);

        await Assert.That(request.Headers.GetValues("Authorization").Single()).IsEqualTo("raw-token");
    }
}
