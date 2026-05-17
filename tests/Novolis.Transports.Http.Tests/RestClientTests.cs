using Novolis.Transports.Http.Abstractions;
using Novolis.Transports.Http.Tests.Infrastructure;
using TUnit.Core;

namespace Novolis.Transports.Http.Tests;

public class RestClientTests
{
    [Test]
    public async Task SendAsync_InvokesAuthenticationsBeforeEnrichers()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);

        var enricher = new OrderEnricher();
        IHttpAuthentication[] auths = [new SetsHeaderAuthentication("X-Phase", "auth")];
        IRequestEnricher[] enrichers = [enricher];

        var sut = new RestClient(http, auths, enrichers);
        await sut.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.test/"), CancellationToken.None);

        await Assert.That(enricher.SawAuthHeader).IsTrue();
        await Assert.That(handler.SentRequests.Count).IsEqualTo(1);
        var req = handler.SentRequests[0];
        await Assert.That(req.Headers.GetValues("X-Phase").Single()).IsEqualTo("auth");
    }

    [Test]
    public async Task SendAsync_RunsMultipleAuthenticationsInOrder()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);

        IHttpAuthentication[] auths =
        [
            new SetsHeaderAuthentication("X-A", "1"),
            new SetsHeaderAuthentication("X-B", "2")
        ];

        var sut = new RestClient(http, auths, []);
        await sut.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.test/"), CancellationToken.None);

        var req = handler.SentRequests[0];
        await Assert.That(req.Headers.GetValues("X-A").Single()).IsEqualTo("1");
        await Assert.That(req.Headers.GetValues("X-B").Single()).IsEqualTo("2");
    }

    [Test]
    public async Task SendAsync_RunsMultipleEnrichersInOrder()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);

        IRequestEnricher[] enrichers =
        [
            new SetsHeaderEnricher("X-E1", "a"),
            new SetsHeaderEnricher("X-E2", "b")
        ];

        var sut = new RestClient(http, [], enrichers);
        await sut.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.test/"), CancellationToken.None);

        var req = handler.SentRequests[0];
        await Assert.That(req.Headers.GetValues("X-E1").Single()).IsEqualTo("a");
        await Assert.That(req.Headers.GetValues("X-E2").Single()).IsEqualTo("b");
    }

    [Test]
    public async Task SendAsync_PassesCancellationTokenToHttpClient()
    {
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (_, ct) =>
                Task.FromResult(ct.IsCancellationRequested
                    ? new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                    : new HttpResponseMessage(System.Net.HttpStatusCode.OK))
        };
        using var http = new HttpClient(handler, disposeHandler: true);
        var sut = new RestClient(http, [], []);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await sut.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.test/"), cts.Token));
    }

    private sealed class SetsHeaderAuthentication(string name, string value) : IHttpAuthentication
    {
        public Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation(name, value);
            return Task.CompletedTask;
        }
    }

    private sealed class SetsHeaderEnricher(string name, string value) : IRequestEnricher
    {
        public void Enrich(HttpRequestMessage request) =>
            request.Headers.TryAddWithoutValidation(name, value);
    }

    private sealed class OrderEnricher : IRequestEnricher
    {
        public bool SawAuthHeader { get; private set; }

        public void Enrich(HttpRequestMessage request)
        {
            SawAuthHeader = request.Headers.Contains("X-Phase");
            request.Headers.TryAddWithoutValidation("X-Enricher", "ok");
        }
    }
}
