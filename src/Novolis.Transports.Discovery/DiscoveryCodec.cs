using System.Text;
using System.Text.Json;

namespace Novolis.Transports.Discovery;

/// <summary>Encodes discovery probes and beacons as UTF-8 JSON datagrams.</summary>
public static class DiscoveryCodec
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    /// <summary>Encodes a probe token.</summary>
    public static byte[] EncodeProbe(string token) =>
        Encoding.UTF8.GetBytes(token ?? throw new ArgumentNullException(nameof(token)));

    /// <summary>Encodes a discovery beacon.</summary>
    public static byte[] EncodeBeacon(DiscoveryBeacon beacon)
    {
        ArgumentNullException.ThrowIfNull(beacon);
        return JsonSerializer.SerializeToUtf8Bytes(beacon, Options);
    }

    /// <summary>Decodes a discovery beacon and returns null for malformed data.</summary>
    public static DiscoveryBeacon? TryDecodeBeacon(ReadOnlySpan<byte> payload)
    {
        try
        {
            return JsonSerializer.Deserialize<DiscoveryBeacon>(payload, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
