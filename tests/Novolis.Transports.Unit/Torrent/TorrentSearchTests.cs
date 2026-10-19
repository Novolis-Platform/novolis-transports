using System.Net;
using System.Text;
using Novolis.Transports.Torrent.Search;

namespace Novolis.Transports.Unit.Torrent;

public sealed class TorrentSearchTests
{
    [Test]
    public async Task Filters_results_by_seeders_leechers_ratio_and_availability()
    {
        var query = new TorrentSearchQuery("linux")
        {
            MinimumSeeders = 5,
            MinimumLeechers = 1,
            MinimumRatio = 2m,
            MinimumAvailability = 1m,
            SortBy = TorrentSearchSort.Seeders,
        };
        var results = TorrentSearchClient.Apply(
            query,
            [
                new TorrentSearchResult("good")
                {
                    Seeders = 10,
                    Leechers = 2,
                    Availability = 4m,
                },
                new TorrentSearchResult("missing availability")
                {
                    Seeders = 10,
                    Leechers = 2,
                },
                new TorrentSearchResult("too few seeders")
                {
                    Seeders = 2,
                    Leechers = 2,
                    Availability = 4m,
                },
            ]);

        await Assert.That(results.Count).IsEqualTo(1);
        await Assert.That(results[0].Name).IsEqualTo("good");
        await Assert.That(results[0].SeederLeecherRatio).IsEqualTo(5m);
    }

    [Test]
    public async Task Torznab_provider_normalizes_xml_results()
    {
        var handler = new XmlHandler(
            """
            <rss xmlns:torznab="http://torznab.com/schemas/2015/feed">
              <channel>
                <item>
                  <title>Ubuntu fixture</title>
                  <guid>https://unit.test/torrent/fixture</guid>
                  <link>magnet:?xt=urn:btih:0123456789abcdef0123456789abcdef01234567</link>
                  <enclosure url="https://unit.test/files/fixture.torrent" length="123456" />
                  <pubDate>Tue, 20 Oct 2026 00:00:00 GMT</pubDate>
                  <category>Linux</category>
                  <torznab:attr name="infohash" value="0123456789abcdef0123456789abcdef01234567" />
                  <torznab:attr name="seeders" value="12" />
                  <torznab:attr name="leechers" value="3" />
                  <torznab:attr name="ratio" value="4.5" />
                  <torznab:attr name="availability" value="2" />
                  <torznab:attr name="grabs" value="99" />
                </item>
              </channel>
            </rss>
            """);
        using var http = new HttpClient(handler);
        var provider = new TorznabSearchProvider(
            http,
            new Uri("https://unit.test/api"),
            apiKey: "secret",
            name: "fixture");

        var results = await provider.SearchAsync(
            new TorrentSearchQuery("ubuntu")
            {
                MinimumSeeders = 10,
                MinimumRatio = 4m,
                MinimumAvailability = 1m,
            });

        await Assert.That(handler.LastRequestUri?.Query)
            .Contains("q=ubuntu");
        await Assert.That(handler.LastRequestUri?.Query)
            .Contains("apikey=secret");
        await Assert.That(results.Count).IsEqualTo(1);
        await Assert.That(results[0].InfoHash)
            .IsEqualTo("0123456789abcdef0123456789abcdef01234567");
        await Assert.That(results[0].TorrentUri?.AbsoluteUri)
            .IsEqualTo("https://unit.test/files/fixture.torrent");
        await Assert.That(results[0].Seeders).IsEqualTo(12);
        await Assert.That(results[0].Leechers).IsEqualTo(3);
        await Assert.That(results[0].EffectiveRatio).IsEqualTo(4.5m);
        await Assert.That(results[0].Availability).IsEqualTo(2m);
        await Assert.That(results[0].CompletedDownloads).IsEqualTo(99);
    }

    private sealed class XmlHandler(string xml) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            this.LastRequestUri = request.RequestUri;
            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        xml,
                        Encoding.UTF8,
                        "application/xml"),
                });
        }
    }
}
