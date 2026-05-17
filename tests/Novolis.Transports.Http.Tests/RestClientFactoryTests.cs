using Novolis.Transports.Http.Abstractions;
using Novolis.Transports.Http.Tests.Infrastructure;
using TUnit.Core;

namespace Novolis.Transports.Http.Tests;

public class RestClientFactoryTests
{
    [Test]
    public async Task CreateClient_Vanilla_SendsWithoutAuthOrEnrichers()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);
        var factory = new TestHttpClientFactory(http);

        IHttpAuthentication[] auths = [new TagAuth("should-not-run")];
        IRequestEnricher[] enrichers = [new TagEnricher("should-not-run")];

        var sut = new RestClientFactory(factory, enrichers, auths);
        var client = sut.CreateClient(vanilla: true);

        await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.test/"), CancellationToken.None);

        var req = handler.SentRequests[0];
        await Assert.That(req.Headers.Contains("X-Tag-Auth")).IsFalse();
        await Assert.That(req.Headers.Contains("X-Tag-Enrich")).IsFalse();
    }

    [Test]
    public async Task CreateClient_Default_IncludesRegisteredAuthAndEnrichers()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);
        var factory = new TestHttpClientFactory(http);

        IHttpAuthentication[] auths = [new TagAuth("auth-ok")];
        IRequestEnricher[] enrichers = [new TagEnricher("enrich-ok")];

        var sut = new RestClientFactory(factory, enrichers, auths);
        var client = sut.CreateClient(vanilla: false);

        await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.test/"), CancellationToken.None);

        var req = handler.SentRequests[0];
        await Assert.That(req.Headers.GetValues("X-Tag-Auth").Single()).IsEqualTo("auth-ok");
        await Assert.That(req.Headers.GetValues("X-Tag-Enrich").Single()).IsEqualTo("enrich-ok");
    }

    [Test]
    public async Task CreateClient_WithExplicitCollections_UsesThoseInstances()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);
        var factory = new TestHttpClientFactory(http);

        var defaultAuths = new[] { new TagAuth("default-auth") };
        var defaultEnrichers = new[] { new TagEnricher("default-enrich") };
        var sut = new RestClientFactory(factory, defaultEnrichers, defaultAuths);

        var explicitAuths = new[] { new TagAuth("explicit-auth") };
        var explicitEnrichers = new[] { new TagEnricher("explicit-enrich") };
        var client = sut.CreateClient(explicitEnrichers, explicitAuths);

        await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.test/"), CancellationToken.None);

        var req = handler.SentRequests[0];
        await Assert.That(req.Headers.GetValues("X-Tag-Auth").Single()).IsEqualTo("explicit-auth");
        await Assert.That(req.Headers.GetValues("X-Tag-Enrich").Single()).IsEqualTo("explicit-enrich");
    }

    private sealed class TestHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class TagAuth(string tag) : IHttpAuthentication
    {
        public Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation("X-Tag-Auth", tag);
            return Task.CompletedTask;
        }
    }

    private sealed class TagEnricher(string tag) : IRequestEnricher
    {
        public void Enrich(HttpRequestMessage request) =>
            request.Headers.TryAddWithoutValidation("X-Tag-Enrich", tag);
    }
}
