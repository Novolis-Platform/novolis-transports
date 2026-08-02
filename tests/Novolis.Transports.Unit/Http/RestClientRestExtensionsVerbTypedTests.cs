using System.Net;
using Novolis.Transports.Http;
using Novolis.Transports.Http.Extensions;
using Novolis.Transports.Http.Tests.Infrastructure;

namespace Novolis.Transports.Unit.Http;

public sealed class RestClientRestExtensionsVerbTypedTests
{
    [Test]
    public async Task Typed_verbs_deserialize_json_responses()
    {
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":42}""", System.Text.Encoding.UTF8, "application/json"),
            }),
        };
        using var http = new HttpClient(handler, disposeHandler: true);
        var client = new RestClient(http, [], []);

        await Assert.That((await client.PutAsync<ValueDto>("https://api.test/p/1", new { a = 1 }, CancellationToken.None))!.Value).IsEqualTo(42);
        await Assert.That((await client.PatchAsync<ValueDto>("https://api.test/p/1", new { a = 2 }, CancellationToken.None))!.Value).IsEqualTo(42);
        await Assert.That((await client.DeleteAsync<ValueDto>("https://api.test/p/1", CancellationToken.None))!.Value).IsEqualTo(42);
        await Assert.That((await client.HeadAsync<ValueDto>("https://api.test/h", CancellationToken.None))!.Value).IsEqualTo(42);
        await Assert.That((await client.OptionsAsync<ValueDto>("https://api.test/o", CancellationToken.None))!.Value).IsEqualTo(42);
        await Assert.That((await client.TraceAsync<ValueDto>("https://api.test/t", CancellationToken.None))!.Value).IsEqualTo(42);
    }

    [Test]
    public async Task SendAsync_null_content_skips_body()
    {
        var handler = new StubHttpMessageHandler();
        using var http = new HttpClient(handler, disposeHandler: true);
        var client = new RestClient(http, [], []);

        await client.SendAsync<object?>(new HttpRequestMessage(HttpMethod.Get, "https://api.test/x"), CancellationToken.None);
        await Assert.That(handler.SentRequests[0].Content).IsNull();
    }

    private sealed record ValueDto(int Value);
}
