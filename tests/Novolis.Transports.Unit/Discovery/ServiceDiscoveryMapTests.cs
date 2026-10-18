using Novolis.Transports.Discovery;

namespace Novolis.Transports.Unit.Discovery;

public sealed class ServiceDiscoveryMapTests
{
    [Test]
    public async Task Find_filters_by_cohort_and_optional_service()
    {
        var map = new ServiceDiscoveryMap();
        map.Replace(
            new[]
            {
                Create("reach", "host", "one"),
                Create("reach", "agent", "two"),
                Create("other", "host", "three"),
            });

        var hosts = map.Find(new ServiceDiscoveryQuery("REACH", "HOST"));
        var cohort = map.Find(new ServiceDiscoveryQuery("reach"));

        await Assert.That(hosts).Count().IsEqualTo(1);
        await Assert.That(hosts[0].InstanceId).IsEqualTo("one");
        await Assert.That(cohort).Count().IsEqualTo(2);
    }

    [Test]
    public async Task Upsert_and_remove_change_the_snapshot()
    {
        var map = new ServiceDiscoveryMap();
        var registration = Create("reach", "host", "one");

        map.Upsert(registration);
        var removed = map.Remove(registration.Key);

        await Assert.That(removed).IsTrue();
        await Assert.That(map.Snapshot).Count().IsEqualTo(0);
    }

    [Test]
    public async Task Refresh_merges_all_sources()
    {
        var registry = new InMemoryServiceRegistry();
        var registration = Create("reach", "host", "one");
        await registry.RegisterAsync(
            registration,
            TimeSpan.FromSeconds(30));

        var map = new ServiceDiscoveryMap();
        await map.RefreshAsync(
            new ServiceDiscoveryQuery("reach"),
            new IServiceDiscoverySource[] { registry });

        await Assert.That(map.Snapshot).Count().IsEqualTo(1);
        await Assert.That(map.Snapshot[0].Key).IsEqualTo(registration.Key);
    }

    private static ServiceRegistration Create(
        string cohort,
        string serviceName,
        string instanceId) =>
        new(
            cohort,
            serviceName,
            instanceId,
            new[]
            {
                new ServiceEndpoint(
                    new Uri($"tcp://{instanceId}.example.test:19800")),
            });
}
