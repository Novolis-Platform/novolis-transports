using Microsoft.Extensions.DependencyInjection;
using Novolis.Transports.WireFish;
using TUnit.Core;

namespace Novolis.Transports.WireFish.Tests;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public async Task AddNovolisWireFish_registers_capture_pipeline()
    {
        var services = new ServiceCollection();
        services.AddNovolisWireFish(builder => builder.AddPacketHandler<NoOpPacketHandler>());

        await Assert.That(services.Count(d => d.ServiceType == typeof(IPacketHandler))).IsEqualTo(1);
        await Assert.That(services.Count(d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService)))
            .IsGreaterThan(0);
    }

    [Test]
    public async Task AddNovolisWireFish_binds_options()
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

        await Assert.That(options.CaptureAllDevices).IsFalse();
        await Assert.That(options.DeviceNames.Contains("eth0")).IsTrue();
        await Assert.That(options.BpfFilter).IsEqualTo("tcp port 443");
    }

    private sealed class NoOpPacketHandler : IPacketHandler
    {
        public bool CanHandle(DevicePacket packet) => false;

        public Task HandleAsync(DevicePacket packet, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
