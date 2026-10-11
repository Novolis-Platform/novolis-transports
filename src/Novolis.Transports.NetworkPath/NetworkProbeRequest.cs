namespace Novolis.Transports.NetworkPath;

/// <summary>Controls one adapter-probing run.</summary>
public sealed record NetworkProbeRequest(
    NetworkProbeTarget Target,
    int Attempts = 3,
    TimeSpan Timeout = default,
    bool ProbeAllAdapters = true)
{
    public TimeSpan EffectiveTimeout =>
        this.Timeout == default ? TimeSpan.FromSeconds(3) : this.Timeout;

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(this.Target);
        if (this.Attempts is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(this.Attempts), "Attempts must be between 1 and 100.");
        if (this.EffectiveTimeout <= TimeSpan.Zero ||
            this.EffectiveTimeout > TimeSpan.FromMinutes(2))
        {
            throw new ArgumentOutOfRangeException(
                nameof(this.Timeout),
                "The probe timeout must be greater than zero and no more than two minutes.");
        }
    }
}
