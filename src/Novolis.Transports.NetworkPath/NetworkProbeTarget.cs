using System.Globalization;

namespace Novolis.Transports.NetworkPath;

/// <summary>A host and TCP port used for an explicit path probe.</summary>
public sealed record NetworkProbeTarget
{
    public NetworkProbeTarget(string host, int port)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        if (host.Any(char.IsWhiteSpace))
            throw new ArgumentException("A probe host cannot contain whitespace.", nameof(host));
        if (port is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(port));

        this.Host = host.Trim();
        this.Port = port;
    }

    public string Host { get; }

    public int Port { get; }

    public static bool TryParse(
        string? text,
        out NetworkProbeTarget? target,
        out string? error)
    {
        target = null;
        error = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            error = "A probe target is required in host:port form.";
            return false;
        }

        var value = text.Trim();
        string host;
        string portText;
        if (value.Length > 0 && value[0] == '[')
        {
            var close = value.IndexOf(']');
            if (close <= 1 ||
                close + 1 >= value.Length ||
                value[close + 1] != ':')
            {
                error = "IPv6 probe targets must use [address]:port form.";
                return false;
            }

            host = value[1..close];
            portText = value[(close + 2)..];
        }
        else
        {
            var firstColon = value.IndexOf(':');
            var lastColon = value.LastIndexOf(':');
            if (firstColon <= 0 || firstColon != lastColon)
            {
                error = "Probe targets must use host:port form; bracket IPv6 addresses.";
                return false;
            }

            host = value[..firstColon];
            portText = value[(firstColon + 1)..];
        }

        if (!int.TryParse(
                portText,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var port) ||
            port is < 1 or > 65535)
        {
            error = "The probe port must be an integer from 1 to 65535.";
            return false;
        }

        try
        {
            target = new NetworkProbeTarget(host, port);
            return true;
        }
        catch (ArgumentException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    public override string ToString() =>
        this.Host.IndexOf(':') >= 0
            ? $"[{this.Host}]:{this.Port.ToString(CultureInfo.InvariantCulture)}"
            : $"{this.Host}:{this.Port.ToString(CultureInfo.InvariantCulture)}";
}
