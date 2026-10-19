using System.Net;
using Novolis.Transports.Torrent.Trackers;

namespace Novolis.Transports.Unit.Torrent;

public sealed class TorrentTrackerTests
{
    [Test]
    public async Task Newtrackon_source_reads_live_tracker_lines()
    {
        var handler = new TextHandler(
            """
            # comment
            udp://tracker.example:6969/announce
            udp://tracker.example:6969/announce
            https://tracker.example/announce
            not-a-tracker
            """);
        using var http = new HttpClient(handler);
        var source = new NewTrackonTrackerSource(
            http,
            new Uri("https://unit.test/"));

        var list = await source.GetAsync(TorrentTrackerListKind.Live);

        await Assert.That(handler.LastRequestUri?.AbsoluteUri)
            .IsEqualTo("https://unit.test/api/live");
        await Assert.That(list.Source)
            .IsEqualTo(TorrentTrackerSourceKind.NewTrackon);
        await Assert.That(list.Trackers.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Catalog_deduplicates_urls_across_sources()
    {
        var shared = new Uri("udp://shared.example:6969/announce");
        var catalog = new TorrentTrackerCatalog(
        [
            new StubSource(
                TorrentTrackerSourceKind.NewTrackon,
                [shared, new Uri("udp://one.example:6969/announce")]),
            new StubSource(
                TorrentTrackerSourceKind.Ngosang,
                [shared, new Uri("udp://two.example:6969/announce")]),
        ]);

        var trackers = await catalog.GetDistinctTrackersAsync();

        await Assert.That(trackers.Count).IsEqualTo(3);
        await Assert.That(catalog[TorrentTrackerSourceKind.Ngosang].Kind)
            .IsEqualTo(TorrentTrackerSourceKind.Ngosang);
    }

    private sealed class TextHandler(string text) : HttpMessageHandler
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
                    Content = new StringContent(text),
                });
        }
    }

    private sealed class StubSource(
        TorrentTrackerSourceKind kind,
        IReadOnlyList<Uri> trackers) : ITorrentTrackerSource
    {
        public TorrentTrackerSourceKind Kind => kind;

        public Task<TorrentTrackerList> GetAsync(
            TorrentTrackerListKind listKind = TorrentTrackerListKind.Best,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                new TorrentTrackerList(
                    this.Kind,
                    listKind,
                    new Uri("https://unit.test/list"),
                    DateTimeOffset.UtcNow,
                    trackers));
        }
    }
}
