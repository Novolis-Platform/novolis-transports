namespace Novolis.Transports.NetworkPath;

/// <summary>Aggregated latency and loss statistics for one adapter.</summary>
public sealed record NetworkProbeStatistics(
    int Attempts,
    int Successes,
    int Failures,
    double LossPercentage,
    TimeSpan? Minimum,
    TimeSpan? Median,
    TimeSpan? Maximum)
{
    public bool IsHealthy => this.Successes > 0;

    public static NetworkProbeStatistics From(IReadOnlyList<NetworkProbeSample> samples)
    {
        ArgumentNullException.ThrowIfNull(samples);

        var durations = samples
            .Where(sample => sample.Succeeded && sample.Duration is not null)
            .Select(sample => sample.Duration!.Value)
            .OrderBy(duration => duration)
            .ToArray();
        var attempts = samples.Count;
        var successes = durations.Length;
        var failures = attempts - successes;
        var loss = attempts == 0 ? 0d : failures * 100d / attempts;

        return new NetworkProbeStatistics(
            attempts,
            successes,
            failures,
            loss,
            durations.Length == 0 ? null : durations[0],
            GetMedian(durations),
            durations.Length == 0 ? null : durations[^1]);
    }

    private static TimeSpan? GetMedian(IReadOnlyList<TimeSpan> durations)
    {
        if (durations.Count == 0)
            return null;
        if (durations.Count % 2 == 1)
            return durations[durations.Count / 2];

        var left = durations[(durations.Count / 2) - 1].Ticks;
        var right = durations[durations.Count / 2].Ticks;
        return TimeSpan.FromTicks(left / 2 + right / 2 + (left % 2 + right % 2) / 2);
    }
}
