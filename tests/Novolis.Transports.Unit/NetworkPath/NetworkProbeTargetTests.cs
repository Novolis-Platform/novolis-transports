using Novolis.Transports.NetworkPath;

namespace Novolis.Transports.Unit.NetworkPath;

public sealed class NetworkProbeTargetTests
{
    [Test]
    public async Task Host_port_roundtrips()
    {
        var parsed = NetworkProbeTarget.TryParse(
            "example.com:443",
            out var target,
            out var error);

        await Assert.That(parsed).IsTrue();
        await Assert.That(error).IsNull();
        await Assert.That(target!.Host).IsEqualTo("example.com");
        await Assert.That(target.Port).IsEqualTo(443);
        await Assert.That(target.ToString()).IsEqualTo("example.com:443");
    }

    [Test]
    public async Task Bracketed_ipv6_roundtrips()
    {
        var parsed = NetworkProbeTarget.TryParse(
            "[2001:db8::1]:8443",
            out var target,
            out var error);

        await Assert.That(parsed).IsTrue();
        await Assert.That(error).IsNull();
        await Assert.That(target!.Host).IsEqualTo("2001:db8::1");
        await Assert.That(target.Port).IsEqualTo(8443);
        await Assert.That(target.ToString()).IsEqualTo("[2001:db8::1]:8443");
    }

    [Test]
    public async Task Invalid_target_reports_a_useful_error()
    {
        var parsed = NetworkProbeTarget.TryParse(
            "2001:db8::1:443",
            out var target,
            out var error);

        await Assert.That(parsed).IsFalse();
        await Assert.That(target).IsNull();
        await Assert.That(error).Contains("bracket");
    }
}
