using System.Net;
using System.Net.NetworkInformation;
using Novolis.Transports.NetworkPath;

namespace Novolis.Transports.Unit.NetworkPath;

public sealed class NetworkPathInspectorTests
{
    [Test]
    public async Task Inspector_classifies_default_path_and_selects_fastest_healthy_adapter()
    {
        var slow = CreateAdapter(
            "slow",
            "Ethernet",
            "192.0.2.10",
            NetworkInterfaceType.Ethernet);
        var fast = CreateAdapter(
            "fast",
            "WireGuard VPN",
            "198.51.100.10",
            NetworkInterfaceType.Tunnel);
        var target = new NetworkProbeTarget("example.com", 443);
        var inspector = new NetworkPathInspector(
            new FakeNetworkAdapterProvider([slow, fast]),
            new FakeNetworkRouteResolver(fast.Addresses[0]),
            new FakeNetworkPathProbe(
                new Dictionary<string, TimeSpan?>
                {
                    [slow.Id] = TimeSpan.FromMilliseconds(30),
                    [fast.Id] = TimeSpan.FromMilliseconds(10)
                }));

        var report = await inspector.InspectAsync(
            new NetworkProbeRequest(target, Attempts: 2));

        await Assert.That(report.DefaultRoute.AdapterId).IsEqualTo(fast.Id);
        await Assert.That(report.Adapters.Single(path => path.Adapter.Id == fast.Id).Kind)
            .IsEqualTo(NetworkPathKind.DefaultEgress);
        await Assert.That(report.Adapters.Single(path => path.Adapter.Id == fast.Id).Evidence)
            .Contains("the selected path has tunnel-like evidence");
        await Assert.That(report.SelectedProbe!.Path.Adapter.Id).IsEqualTo(fast.Id);
        await Assert.That(report.SelectedProbe.Statistics.Median)
            .IsEqualTo(TimeSpan.FromMilliseconds(10));
        await Assert.That(report.SelectedProbe.Statistics.Attempts).IsEqualTo(2);
    }

    [Test]
    public async Task Inspector_can_limit_probes_to_the_default_path()
    {
        var first = CreateAdapter(
            "first",
            "Ethernet",
            "192.0.2.11",
            NetworkInterfaceType.Ethernet);
        var second = CreateAdapter(
            "second",
            "Wi-Fi",
            "192.0.2.12",
            NetworkInterfaceType.Wireless80211);
        var inspector = new NetworkPathInspector(
            new FakeNetworkAdapterProvider([first, second]),
            new FakeNetworkRouteResolver(first.Addresses[0]),
            new FakeNetworkPathProbe(
                new Dictionary<string, TimeSpan?>
                {
                    [first.Id] = TimeSpan.FromMilliseconds(20),
                    [second.Id] = TimeSpan.FromMilliseconds(5)
                }));

        var report = await inspector.InspectAsync(
            new NetworkProbeRequest(
                new NetworkProbeTarget("example.com", 443),
                Attempts: 1,
                ProbeAllAdapters: false));

        await Assert.That(report.Probes.Count).IsEqualTo(1);
        await Assert.That(report.Probes[0].Path.Adapter.Id).IsEqualTo(first.Id);
    }

    private static NetworkAdapterInfo CreateAdapter(
        string id,
        string name,
        string address,
        NetworkInterfaceType interfaceType) =>
        new(
            id,
            name,
            name,
            interfaceType,
            OperationalStatus.Up,
            id.GetHashCode(),
            [IPAddress.Parse(address)],
            [IPAddress.Parse("192.0.2.1")]);
}
