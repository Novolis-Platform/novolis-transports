using System.Net;
using System.Text;
using Novolis.Transports.Tcp;
using Novolis.Transports.Udp;

namespace Novolis.Transports.Unit.Transport;

public sealed class TransportAdapterTests
{
    [Test]
    public async Task Tcp_adapter_round_trips_one_stream()
    {
        await using var listener = new TcpTransportListener(
            new IPEndPoint(IPAddress.Loopback, 0));
        var acceptTask = listener.AcceptConnectionAsync().AsTask();
        await using var client = await TcpTransportConnection.ConnectAsync(
            (IPEndPoint)listener.LocalEndPoint);
        await using var clientStream =
            await client.OpenBidirectionalStreamAsync();
        await using var server = await acceptTask;
        await using var serverStream =
            await server.AcceptInboundStreamAsync();

        var payload = Encoding.UTF8.GetBytes("reach-transport");
        await clientStream.Stream.WriteAsync(payload);
        await clientStream.Stream.FlushAsync();
        var received = new byte[payload.Length];
        var offset = 0;
        while (offset < received.Length)
        {
            var read = await serverStream.Stream.ReadAsync(
                received.AsMemory(offset));
            if (read == 0)
                break;
            offset += read;
        }

        await Assert.That(received).IsEquivalentTo(payload);
    }

    [Test]
    public async Task Udp_adapter_round_trips_a_bounded_datagram()
    {
        await using var receiver = new UdpDatagramChannel(
            new IPEndPoint(IPAddress.Loopback, 0));
        await using var sender = new UdpDatagramChannel(
            new IPEndPoint(IPAddress.Loopback, 0));
        var payload = Encoding.UTF8.GetBytes("reach-datagram");

        await sender.SendAsync(payload, receiver.LocalEndPoint);
        var datagram = await receiver.ReceiveAsync();

        await Assert.That(datagram.Payload).IsEquivalentTo(payload);
        await Assert.That(datagram.RemoteEndpoint).IsEqualTo(sender.LocalEndPoint);
    }
}
