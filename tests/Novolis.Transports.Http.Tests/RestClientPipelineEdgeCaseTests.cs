using Novolis.Transports.Http;
using Novolis.Transports.Http.Tests.Infrastructure;
using TUnit.Core;

namespace Novolis.Transports.Http.Tests;

public class RestClientPipelineEdgeCaseTests
{
    [Test]
    public async Task SendAsync_WithEmptyAuthenticationsAndEnrichers_StillCompletes()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);
        var sut = new RestClient(http, [], []);

        var response = await sut.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.test/"), CancellationToken.None);

        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(handler.SentRequests.Count).IsEqualTo(1);
    }

    [Test]
    public async Task SendAsync_WithRelativeUri_PassesThroughToHttpClient()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true)
        {
            BaseAddress = new Uri("https://api.example/")
        };
        var sut = new RestClient(http, [], []);

        await sut.SendAsync(new HttpRequestMessage(HttpMethod.Get, "v1/items"), CancellationToken.None);

        await Assert.That(handler.SentRequests[0].RequestUri!.ToString()).IsEqualTo("https://api.example/v1/items");
    }
}
