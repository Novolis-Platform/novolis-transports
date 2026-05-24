using Novolis.Transports.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;

namespace Novolis.Transports.Http.Tests;

public class AuthenticationBuilderTests
{
    [Test]
    public async Task AddAuthentication_RegistersConcreteTypeAndIHttpAuthentication()
    {
        var services = new ServiceCollection();
        var builder = new AuthenticationBuilder(services);
        builder.AddAuthentication<SampleAuth>();

        await using var provider = services.BuildServiceProvider();
        var concrete = provider.GetRequiredService<SampleAuth>();
        var abstraction = provider.GetRequiredService<IHttpAuthentication>();

        await Assert.That(ReferenceEquals(concrete, abstraction)).IsTrue();
    }

    [Test]
    public async Task AddAuthentication_WithInstance_RegistersSameSingleton()
    {
        var services = new ServiceCollection();
        var instance = new SampleAuth();
        var builder = new AuthenticationBuilder(services);
        builder.AddAuthentication(instance);

        await using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IHttpAuthentication>();

        await Assert.That(ReferenceEquals(resolved, instance)).IsTrue();
    }

    [Test]
    public async Task AddAuthentication_CalledTwice_RegistersLastAsIHttpAuthentication()
    {
        var services = new ServiceCollection();
        var builder = new AuthenticationBuilder(services);
        builder.AddAuthentication<SampleAuth>();
        builder.AddAuthentication<OtherAuth>();

        await using var provider = services.BuildServiceProvider();
        var auth = provider.GetRequiredService<IHttpAuthentication>();

        await Assert.That(auth).IsTypeOf<OtherAuth>();
    }

    private sealed class SampleAuth : IHttpAuthentication
    {
        public Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class OtherAuth : IHttpAuthentication
    {
        public Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
