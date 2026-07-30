namespace Novolis.Transports.WireFish;

/// <summary>UI-agnostic protocol-detail tree node (no PacketDotNet types).</summary>
public sealed class PacketDetailNode
{
    /// <summary>Creates a detail node with optional description and children.</summary>
    public PacketDetailNode(string title, string? description = null, IReadOnlyList<PacketDetailNode>? children = null)
    {
        Title = title;
        Description = description;
        Children = children ?? [];
    }

    /// <summary>Primary label for this layer (e.g. type name and length).</summary>
    public string Title { get; }

    /// <summary>Optional human-readable field summary for the layer.</summary>
    public string? Description { get; }

    /// <summary>Nested payload layers in outer-to-inner order as a single child chain.</summary>
    public IReadOnlyList<PacketDetailNode> Children { get; }
}
