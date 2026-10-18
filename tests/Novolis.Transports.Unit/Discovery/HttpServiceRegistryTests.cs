using System.Net;
using System.Net.Http;
using Novolis.Transports.Discovery;

namespace Novolis.Transports.Unit.Discovery;

public sealed class HttpServiceRegistryTests
{
    [Test]
    public async Task Discover_reads_the_cohort_map_from_the_registry()
    {
        var handler = new RegistryHandler
        {
            ResponseJson = """
                [
                  {
                    "cohort": "reach",
                    "serviceName": "host",
                    "instanceId": "host-one",
                    "endpoints": [
                      {
                        "address": "tcp://10.0.0.25:19800",
                        "priority": 0,
                        "weight": 10
                      }
                    ],
                    "metadata": {
                      "hostName": "host-one",
                      "protocolVersion": "1.0"
                    }
                  }
                ]
                """,
        };
        using var http = new HttpClient(handler);
        var registry = new HttpServiceRegistry(
            http,
            new Uri("https://registry.example.test/"));

        var results = await registry.DiscoverAsync(
            new ServiceDiscoveryQuery("reach", "host"));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0].InstanceId).IsEqualTo("host-one");
        await Assert.That(results[0].Metadata["hostName"])
            .IsEqualTo("host-one");
        await Assert.That(handler.LastRequest!.RequestUri!.AbsolutePath)
            .IsEqualTo("/v1/services");
        await Assert.That(handler.LastRequest.RequestUri.Query)
            .Contains("cohort=reach");
    }

    [Test]
    public async Task Register_uses_an_idempotent_lease_put()
    {
        var handler = new RegistryHandler();
        using var http = new HttpClient(handler);
        var registry = new HttpServiceRegistry(
            http,
            new Uri("https://registry.example.test/"));
        var registration = new ServiceRegistration(
            "reach",
            "host",
            "host-one",
            new[]
            {
                new ServiceEndpoint(new Uri("tcp://10.0.0.25:19800")),
            });

        await registry.RegisterAsync(
            registration,
            TimeSpan.FromSeconds(30));

        await Assert.That(handler.LastRequest!.Method)
            .IsEqualTo(HttpMethod.Put);
        await Assert.That(handler.LastRequest.RequestUri!.AbsolutePath)
            .IsEqualTo("/v1/registrations/reach/host/host-one");
        await Assert.That(handler.LastBody!).Contains(
            "\"leaseDurationSeconds\":30");
    }

    private sealed class RegistryHandler : HttpMessageHandler
    {
        internal HttpRequestMessage? LastRequest { get; private set; }
        internal string? LastBody { get; private set; }
        internal string? ResponseJson { get; init; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    ResponseJson ?? string.Empty),
            };
        }
    }
}
