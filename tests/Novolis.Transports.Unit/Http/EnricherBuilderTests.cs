using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;

namespace Novolis.Transports.Http.Tests;

public class EnricherBuilderTests
{
    [Test]
    public async Task AddEnricher_RegistersConcreteTypeAndIRequestEnricher()
    {
        var services = new ServiceCollection();
        var builder = new EnricherBuilder(services);
        builder.AddEnricher<SampleEnricher>();

        await using var provider = services.BuildServiceProvider();
        var concrete = provider.GetRequiredService<SampleEnricher>();
        var abstraction = provider.GetRequiredService<IRequestEnricher>();

        await Assert.That(ReferenceEquals(concrete, abstraction)).IsTrue();
    }

    [Test]
    public async Task AddEnricher_WithInstance_RegistersSameSingleton()
    {
        var services = new ServiceCollection();
        var instance = new SampleEnricher();
        var builder = new EnricherBuilder(services);
        builder.AddEnricher(instance);

        await using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IRequestEnricher>();

        await Assert.That(ReferenceEquals(resolved, instance)).IsTrue();
    }

    [Test]
    public async Task AddEnricher_CalledTwice_ResolvesBothViaEnumerable()
    {
        var services = new ServiceCollection();
        var builder = new EnricherBuilder(services);
        builder.AddEnricher<EnricherA>();
        builder.AddEnricher<EnricherB>();

        await using var provider = services.BuildServiceProvider();
        var all = provider.GetServices<IRequestEnricher>().ToList();

        await Assert.That(all.Count).IsEqualTo(2);
        await Assert.That(all.OfType<EnricherA>().Single()).IsNotNull();
        await Assert.That(all.OfType<EnricherB>().Single()).IsNotNull();
    }

    private sealed class SampleEnricher : IRequestEnricher
    {
        public void Enrich(HttpRequestMessage request)
        {
        }
    }

    private sealed class EnricherA : IRequestEnricher
    {
        public void Enrich(HttpRequestMessage request) => request.Headers.TryAddWithoutValidation("X-A", "1");
    }

    private sealed class EnricherB : IRequestEnricher
    {
        public void Enrich(HttpRequestMessage request) => request.Headers.TryAddWithoutValidation("X-B", "2");
    }
}
