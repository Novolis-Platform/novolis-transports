namespace Novolis.Transports.NetworkPath;

/// <summary>Inventory, route resolution, and probe results for one destination.</summary>
public sealed record NetworkPathReport(
    DateTimeOffset CapturedAt,
    NetworkProbeTarget Target,
    NetworkRoute DefaultRoute,
    IReadOnlyList<NetworkAdapterPath> Adapters,
    IReadOnlyList<NetworkProbeResult> Probes,
    NetworkProbeResult? SelectedProbe);
