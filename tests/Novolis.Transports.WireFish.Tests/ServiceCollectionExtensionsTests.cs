using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Novolis.Transports.WireFish;
using TUnit.Core;

namespace Novolis.Transports.WireFish.Tests;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddNovolisWireFish_registers_capture_pipeline()
    {
        var services = new ServiceCollection();
        services.AddNovolisWireFish(builder => builder.AddPacketHandler<NoOpPacketHandler>());

        services.Count(d => d.ServiceType == typeof(IPacketHandler)).Should().Be(1);
        services.Count(d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService))
            .Should().BeGreaterThan(0);
    }

    [Test]
    public void AddNovolisWireFish_binds_options()
    {
        var services = new ServiceCollection();
        services.AddNovolisWireFish(
            _ => { },
            o =>
            {
                o.CaptureAllDevices = false;
                o.DeviceNames.Add("eth0");
                o.BpfFilter = "tcp port 443";
            });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<WireFishOptions>>().Value;

        options.CaptureAllDevices.Should().BeFalse();
        options.DeviceNames.Should().Contain("eth0");
        options.BpfFilter.Should().Be("tcp port 443");
    }

    private sealed class NoOpPacketHandler : IPacketHandler
    {
        public bool CanHandle(DevicePacket packet) => false;

        public Task HandleAsync(DevicePacket packet, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
