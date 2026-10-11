namespace Novolis.Transports.Ed2k;

/// <summary>
/// An eD2k server endpoint.
/// </summary>
public sealed record Ed2kServerEndpoint(string Host, int Port)
{
    public override string ToString() => $"{Host}:{Port}";
}
