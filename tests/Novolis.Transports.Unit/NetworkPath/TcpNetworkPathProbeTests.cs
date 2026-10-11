using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Novolis.Transports.NetworkPath;

namespace Novolis.Transports.Unit.NetworkPath;

public sealed class TcpNetworkPathProbeTests
{
    [Test]
    public async Task Probe_binds_and_connects_through_a_loopback_adapter()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var acceptTask = listener.AcceptTcpClientAsync();
        var adapter = new NetworkAdapterInfo(
            "loopback",
            "Loopback",
            "Loopback",
            NetworkInterfaceType.Loopback,
            OperationalStatus.Up,
            1,
            [IPAddress.Loopback],
            []);

        var sample = await new TcpNetworkPathProbe().ProbeAsync(
            adapter,
            new NetworkProbeTarget("127.0.0.1", endpoint.Port),
            TimeSpan.FromSeconds(2));
        using var accepted = await acceptTask.WaitAsync(TimeSpan.FromSeconds(3));

        await Assert.That(sample.Succeeded).IsTrue();
        await Assert.That(sample.Duration).IsNotNull();
        await Assert.That(sample.SourceAddress).IsEqualTo(IPAddress.Loopback);
        await Assert.That(sample.Error).IsNull();
    }
}
