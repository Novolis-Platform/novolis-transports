namespace Novolis.Transports.WireFish;

/// <summary>Handles captured <see cref="DevicePacket"/> instances from the WireFish pipeline.</summary>
public interface IPacketHandler
{
    /// <summary>Processes a captured packet.</summary>
    /// <param name="packet">Captured packet.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when handling finishes.</returns>
    Task HandleAsync(DevicePacket packet, CancellationToken cancellationToken);

    /// <summary>Returns whether this handler should process <paramref name="packet"/>.</summary>
    /// <param name="packet">Captured packet.</param>
    /// <returns><see langword="true"/> when this handler should run.</returns>
    bool CanHandle(DevicePacket packet);
}
