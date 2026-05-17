using System.Net;
using System.Text;
using System.Text.Json;
using Novolis.Transports.Http.Authentication.Oidc;
using Novolis.Transports.Http.Tests.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TUnit.Core;

namespace Novolis.Transports.Http.Tests;

public class OidcTokenProviderTests
{
    [Test]
    public async Task GetTokenAsync_PostsClientCredentialsAndReturnsAccessToken()
    {
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (req, _) =>
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """{"access_token":"server-token","token_type":"Bearer","expires_in":3600}""",
                        Encoding.UTF8,
                        "application/json")
                });
            }
        };
        using var http = new HttpClient(handler, disposeHandler: true);
        var options = Options.Create(new OidcAuthenticationConfiguration
        {
            ClientId = "cid",
            ClientSecret = "sec",
            Scope = "api.read",
            TokenEndpoint = "https://idp.test/oauth/token"
        });
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new OidcTokenProvider(http, options, cache);

        var token = await sut.GetTokenAsync(CancellationToken.None);

        await Assert.That(token).IsEqualTo("server-token");
        await Assert.That(handler.SentRequests.Count).IsEqualTo(1);
        var posted = handler.SentRequests[0];
        await Assert.That(posted.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(posted.RequestUri!.ToString()).IsEqualTo("https://idp.test/oauth/token");
        var form = await posted.Content!.ReadAsStringAsync(CancellationToken.None);
        await Assert.That(form).Contains("client_id=cid");
        await Assert.That(form).Contains("client_secret=sec");
        await Assert.That(form).Contains("scope=api.read");
        await Assert.That(form).Contains("grant_type=client_credentials");
    }

    [Test]
    public async Task GetTokenAsync_SecondCallUsesMemoryCache_WithoutSecondHttpRequest()
    {
        var callCount = 0;
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (_, _) =>
            {
                Interlocked.Increment(ref callCount);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"access_token":"once"}""", Encoding.UTF8, "application/json")
                });
            }
        };
        using var http = new HttpClient(handler, disposeHandler: true);
        var options = Options.Create(new OidcAuthenticationConfiguration
        {
            ClientId = "a",
            ClientSecret = "b",
            Scope = "c",
            TokenEndpoint = "https://idp.test/token"
        });
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new OidcTokenProvider(http, options, cache);

        var first = await sut.GetTokenAsync(CancellationToken.None);
        var second = await sut.GetTokenAsync(CancellationToken.None);

        await Assert.That(first).IsEqualTo("once");
        await Assert.That(second).IsEqualTo("once");
        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task GetTokenAsync_WhenTokenEndpointMissing_ThrowsInvalidOperationException()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);
        var options = Options.Create(new OidcAuthenticationConfiguration
        {
            ClientId = "a",
            ClientSecret = "b",
            Scope = "c",
            TokenEndpoint = null
        });
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new OidcTokenProvider(http, options, cache);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.GetTokenAsync(CancellationToken.None));
        await Assert.That(ex!.Message).Contains("TokenEndpoint");
    }

    [Test]
    public async Task GetTokenAsync_WhenBodyIsNotJson_ThrowsJsonException()
    {
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-json", Encoding.UTF8, "text/plain")
            })
        };
        using var http = new HttpClient(handler, disposeHandler: true);
        var options = Options.Create(new OidcAuthenticationConfiguration
        {
            ClientId = "a",
            ClientSecret = "b",
            Scope = "c",
            TokenEndpoint = "https://idp.test/token"
        });
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new OidcTokenProvider(http, options, cache);

        await Assert.ThrowsAsync<JsonException>(async () => await sut.GetTokenAsync(CancellationToken.None));
    }
}
