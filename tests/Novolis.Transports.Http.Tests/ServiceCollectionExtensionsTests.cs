using Novolis.Transports.Http;
using Novolis.Transports.Http.Abstractions;
using Novolis.Transports.Http.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;

namespace Novolis.Transports.Http.Tests;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public async Task AddFrankHttp_RegistersFactoryAndTransientRestClient()
    {
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddFrankHttp();

        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IRestClientFactory>();
        var client1 = provider.GetRequiredService<IRestClient>();
        var client2 = provider.GetRequiredService<IRestClient>();

        await Assert.That(ReferenceEquals(client1, client2)).IsFalse();
        await Assert.That(factory.CreateClient()).IsNotNull();
    }

    [Test]
    public async Task AddFrankHttp_WithEnricherAction_RegistersEnricher()
    {
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddFrankHttp(b => b.AddEnricher<HeaderEnricher>());

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IRestClient>();
        await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.test/"), CancellationToken.None);

        await Assert.That(handler.SentRequests[0].Headers.GetValues("X-Test-Enrich").Single()).IsEqualTo("yes");
    }

    [Test]
    public async Task AddFrankHttp_WithAuthenticationAction_RegistersAuthentication()
    {
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddFrankHttp(b => b.AddAuthentication<MarkerAuth>());

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IRestClient>();
        await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.test/"), CancellationToken.None);

        await Assert.That(handler.SentRequests[0].Headers.GetValues("X-Marker-Auth").Single()).IsEqualTo("1");
    }

    [Test]
    public async Task AddFrankHttp_WithBothConfigureActions_AppliesAuthenticationAndEnrichers()
    {
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddFrankHttp(
            e => e.AddEnricher<HeaderEnricher>(),
            a => a.AddAuthentication<MarkerAuth>());

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IRestClient>();
        await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.test/"), CancellationToken.None);

        var req = handler.SentRequests[0];
        await Assert.That(req.Headers.Contains("X-Marker-Auth")).IsTrue();
        await Assert.That(req.Headers.Contains("X-Test-Enrich")).IsTrue();
    }

    [Test]
    public async Task AddFrankHttpAuthentication_RegistersConcreteAuthentication()
    {
        var services = new ServiceCollection();
        services.AddFrankHttpAuthentication<MarkerAuth>();

        await using var provider = services.BuildServiceProvider();
        var auth = provider.GetRequiredService<IHttpAuthentication>();

        await Assert.That(auth).IsTypeOf<MarkerAuth>();
    }

    [Test]
    public async Task AddFrankHttpRequestEnricher_RegistersConcreteEnricher()
    {
        var services = new ServiceCollection();
        services.AddFrankHttpRequestEnricher<HeaderEnricher>();

        await using var provider = services.BuildServiceProvider();
        var enricher = provider.GetRequiredService<IRequestEnricher>();

        await Assert.That(enricher).IsTypeOf<HeaderEnricher>();
    }

    private sealed class HeaderEnricher : IRequestEnricher
    {
        public void Enrich(HttpRequestMessage request) =>
            request.Headers.TryAddWithoutValidation("X-Test-Enrich", "yes");
    }

    private sealed class MarkerAuth : IHttpAuthentication
    {
        public Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation("X-Marker-Auth", "1");
            return Task.CompletedTask;
        }
    }
}
