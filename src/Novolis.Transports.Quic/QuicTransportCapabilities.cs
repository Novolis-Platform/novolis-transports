using System.Net.Quic;
using System.Runtime.Versioning;
using Novolis.Transports;

namespace Novolis.Transports.Quic;

/// <summary>Runtime capability probe for the System.Net.Quic adapter.</summary>
[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public static class QuicTransportCapabilities
{
    /// <summary>Gets whether the current runtime can create QUIC streams.</summary>
    public static bool SupportsStreams =>
        QuicConnection.IsSupported && QuicListener.IsSupported;

    /// <summary>
    /// Gets whether native QUIC datagrams are available through this target
    /// framework. .NET 10 exposes streams only.
    /// </summary>
    public static bool SupportsDatagrams => false;

    /// <summary>Gets the reliable capabilities exposed by this adapter.</summary>
    public static TransportCapabilities AvailableCapabilities =>
        SupportsStreams
            ? TransportCapabilities.ReliableOrderedStream
                | TransportCapabilities.Secure
                | TransportCapabilities.Multiplexed
                | TransportCapabilities.ConnectionMigration
            : TransportCapabilities.None;
}
