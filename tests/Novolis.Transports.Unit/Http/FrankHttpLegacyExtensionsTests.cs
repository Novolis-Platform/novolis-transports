using Novolis.Transports.Http;
using Novolis.Transports.Http.Abstractions;
using Novolis.Transports.Http.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Unit.Http;

public sealed class FrankHttpLegacyExtensionsTests
{
    [Test]
    [Obsolete("Covers Frank-prefixed HTTP DI aliases.")]
    public async Task Frank_prefixed_overloads_register_same_services()
    {
#pragma warning disable CS0618
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddFrankHttp(
            e => e.AddEnricher<HeaderEnricher>(),
            a => a.AddAuthentication<MarkerAuth>());
        services.AddFrankHttpAuthentication<MarkerAuth>();
        services.AddFrankHttpRequestEnricher<HeaderEnricher>();
#pragma warning restore CS0618

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IRestClient>();
        await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.test/"), CancellationToken.None);

        var req = handler.SentRequests[0];
        await Assert.That(req.Headers.Contains("X-Marker-Auth")).IsTrue();
        await Assert.That(req.Headers.Contains("X-Test-Enrich")).IsTrue();
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
