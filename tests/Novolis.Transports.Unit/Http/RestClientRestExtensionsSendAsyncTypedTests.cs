using System.Net;
using Novolis.Transports.Http;
using Novolis.Transports.Http.Extensions;
using Novolis.Transports.Http.Tests.Infrastructure;
using TUnit.Core;

namespace Novolis.Transports.Http.Tests;

public class RestClientRestExtensionsSendAsyncTypedTests
{
    [Test]
    public async Task SendAsync_Generic_WithHttpRequestMessage_DeserializesBody()
    {
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":99}""", System.Text.Encoding.UTF8, "application/json")
            })
        };
        using var http = new HttpClient(handler, disposeHandler: true);
        var client = new RestClient(http, [], []);

        var dto = await client.SendAsync<PayloadDto>(
            new HttpRequestMessage(HttpMethod.Get, "https://api.test/x"),
            CancellationToken.None);

        await Assert.That(dto).IsNotNull();
        await Assert.That(dto!.Value).IsEqualTo(99);
    }

    [Test]
    public async Task SendAsync_Generic_WithMethodUriAndContent_BuildsRequest()
    {
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":1}""", System.Text.Encoding.UTF8, "application/json")
            })
        };
        using var http = new HttpClient(handler, disposeHandler: true);
        var client = new RestClient(http, [], []);

        var dto = await client.SendAsync<PayloadDto>(HttpMethod.Post, "https://api.test/submit", new { a = 1 }, CancellationToken.None);

        var req = handler.SentRequests[0];
        await Assert.That(req.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(req.RequestUri!.ToString()).IsEqualTo("https://api.test/submit");
        await Assert.That(req.Content).IsNotNull();
        await Assert.That(dto!.Value).IsEqualTo(1);
    }

    private sealed record PayloadDto(int Value);
}
