using Novolis.Transports.Http;
using Novolis.Transports.Http.Extensions;
using Novolis.Transports.Http.Tests.Infrastructure;
using Novolis.Transports.LocalIpc;

namespace Novolis.Transports.Unit.LocalIpc;

public sealed class HttpCoverageGapTests
{
    [Test]
    public async Task PostAsync_untyped_returns_response_message()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);
        var client = new RestClient(http, [], []);

        using var response = await client.PostAsync("https://api.test/echo", new { n = 1 }, CancellationToken.None);
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(handler.SentRequests[0].Method).IsEqualTo(HttpMethod.Post);
    }
}
