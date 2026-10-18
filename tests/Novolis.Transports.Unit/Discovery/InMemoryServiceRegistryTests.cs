using Novolis.Transports.Discovery;

namespace Novolis.Transports.Unit.Discovery;

public sealed class InMemoryServiceRegistryTests
{
    [Test]
    public async Task Registration_is_visible_to_matching_cohort_queries()
    {
        var registry = new InMemoryServiceRegistry();
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
        var found = await registry.DiscoverAsync(
            new ServiceDiscoveryQuery("reach", "host"));

        await Assert.That(found).Count().IsEqualTo(1);
        await Assert.That(found[0].Key).IsEqualTo(registration.Key);
    }

    [Test]
    public async Task Unregister_removes_the_instance()
    {
        var registry = new InMemoryServiceRegistry();
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
        await registry.UnregisterAsync(registration.Key);

        var found = await registry.DiscoverAsync(
            new ServiceDiscoveryQuery("reach", "host"));
        await Assert.That(found).Count().IsEqualTo(0);
    }
}
