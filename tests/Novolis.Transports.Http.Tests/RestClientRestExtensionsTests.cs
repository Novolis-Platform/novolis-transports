using System.Net;
using Novolis.Transports.Http;
using Novolis.Transports.Http.Extensions;
using Novolis.Transports.Http.Tests.Infrastructure;
using TUnit.Core;

namespace Novolis.Transports.Http.Tests;

public class RestClientRestExtensionsTests
{
    [Test]
    public async Task GetAsync_Typed_DeserializesJsonWithWebDefaults()
    {
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"id":7,"title":"hello"}""", System.Text.Encoding.UTF8, "application/json")
            })
        };
        using var http = new HttpClient(handler, disposeHandler: true);
        var client = new RestClient(http, [], []);

        var dto = await client.GetAsync<PostDto>("https://api.test/posts/1", CancellationToken.None);

        await Assert.That(dto).IsNotNull();
        await Assert.That(dto!.Id).IsEqualTo(7);
        await Assert.That(dto.Title).IsEqualTo("hello");
        await Assert.That(handler.SentRequests[0].Method).IsEqualTo(HttpMethod.Get);
    }

    [Test]
    public async Task GetAsync_Response_ReturnsHttpResponseMessage()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);
        var client = new RestClient(http, [], []);

        var response = await client.GetAsync("https://api.test/x", CancellationToken.None);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(handler.SentRequests[0].RequestUri!.ToString()).IsEqualTo("https://api.test/x");
    }

    [Test]
    public async Task PostAsync_Typed_SendsJsonContent()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);
        var client = new RestClient(http, [], []);

        handler.SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":1,"title":"created"}""", System.Text.Encoding.UTF8, "application/json")
        });

        var body = new { title = "new" };
        var dto = await client.PostAsync<PostDto>("https://api.test/posts", body, CancellationToken.None);

        var req = handler.SentRequests[0];
        await Assert.That(req.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(req.Content).IsNotNull();
        var json = await req.Content!.ReadAsStringAsync(CancellationToken.None);
        await Assert.That(json).Contains("title");
        await Assert.That(json).Contains("new");
        await Assert.That(dto!.Title).IsEqualTo("created");
    }

    [Test]
    public async Task SendAsync_Typed_WhenNotSuccess_ThrowsHttpRequestException()
    {
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound))
        };
        using var http = new HttpClient(handler, disposeHandler: true);
        var client = new RestClient(http, [], []);

        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await client.GetAsync<PostDto>("https://api.test/missing", CancellationToken.None));
    }

    [Test]
    public async Task PutAsync_PatchAsync_DeleteAsync_UseExpectedMethods()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);
        var client = new RestClient(http, [], []);

        await client.PutAsync("https://api.test/p/1", new { a = 1 }, CancellationToken.None);
        await Assert.That(handler.SentRequests[^1].Method).IsEqualTo(HttpMethod.Put);

        await client.PatchAsync("https://api.test/p/1", new { a = 2 }, CancellationToken.None);
        await Assert.That(handler.SentRequests[^1].Method).IsEqualTo(new HttpMethod("PATCH"));

        await client.DeleteAsync("https://api.test/p/1", CancellationToken.None);
        await Assert.That(handler.SentRequests[^1].Method).IsEqualTo(HttpMethod.Delete);

        await client.HeadAsync("https://api.test/h", CancellationToken.None);
        await Assert.That(handler.SentRequests[^1].Method).IsEqualTo(HttpMethod.Head);

        await client.OptionsAsync("https://api.test/o", CancellationToken.None);
        await Assert.That(handler.SentRequests[^1].Method).IsEqualTo(HttpMethod.Options);

        await client.TraceAsync("https://api.test/t", CancellationToken.None);
        await Assert.That(handler.SentRequests[^1].Method).IsEqualTo(HttpMethod.Trace);
    }

    [Test]
    public async Task SendAsync_WithMethodUriAndNullContent_OmitsBody()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);
        var client = new RestClient(http, [], []);

        await client.SendAsync(HttpMethod.Get, "https://api.test/q", null, CancellationToken.None);

        await Assert.That(handler.SentRequests[0].Content).IsNull();
    }

    private sealed record PostDto(int Id, string Title);
}
