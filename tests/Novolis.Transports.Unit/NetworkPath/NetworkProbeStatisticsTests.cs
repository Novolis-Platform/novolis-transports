using Novolis.Transports.NetworkPath;

namespace Novolis.Transports.Unit.NetworkPath;

public sealed class NetworkProbeStatisticsTests
{
    [Test]
    public async Task Statistics_calculate_loss_and_median()
    {
        var samples = new[]
        {
            new NetworkProbeSample(TimeSpan.FromMilliseconds(10), true, null, null),
            new NetworkProbeSample(null, false, null, "timeout"),
            new NetworkProbeSample(TimeSpan.FromMilliseconds(30), true, null, null),
            new NetworkProbeSample(TimeSpan.FromMilliseconds(20), true, null, null)
        };

        var statistics = NetworkProbeStatistics.From(samples);

        await Assert.That(statistics.Attempts).IsEqualTo(4);
        await Assert.That(statistics.Successes).IsEqualTo(3);
        await Assert.That(statistics.Failures).IsEqualTo(1);
        await Assert.That(statistics.LossPercentage).IsEqualTo(25d);
        await Assert.That(statistics.Minimum).IsEqualTo(TimeSpan.FromMilliseconds(10));
        await Assert.That(statistics.Median).IsEqualTo(TimeSpan.FromMilliseconds(20));
        await Assert.That(statistics.Maximum).IsEqualTo(TimeSpan.FromMilliseconds(30));
    }

    [Test]
    public async Task Empty_statistics_are_not_healthy()
    {
        var statistics = NetworkProbeStatistics.From([]);

        await Assert.That(statistics.IsHealthy).IsFalse();
        await Assert.That(statistics.Median).IsNull();
        await Assert.That(statistics.LossPercentage).IsEqualTo(0d);
    }
}
