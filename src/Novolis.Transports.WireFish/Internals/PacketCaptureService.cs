using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace Novolis.Transports.WireFish.Internals;

internal sealed class PacketCaptureService(
    ILogger<PacketCaptureService> logger,
    PacketHandler packetHandler,
    IOptions<WireFishOptions> options) : IHostedService
{
    private readonly List<LibPcapLiveDevice> _openedDevices = [];
    private readonly WireFishOptions _options = options.Value;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var devices = SelectDevices().ToList();
        if (devices.Count == 0)
        {
            var message = "No packet capture devices are available. Install Npcap/libpcap or adjust WireFishOptions.DeviceNames.";
            if (_options.AllowNoCaptureDevices)
            {
                logger.LogWarning("{Message}", message);
                return Task.CompletedTask;
            }

            throw new InvalidOperationException(message);
        }

        foreach (var device in devices)
        {
            try
            {
                if (_options.PromiscuousMode)
                    device.Open(DeviceModes.Promiscuous);
                else
                    device.Open();
                if (!string.IsNullOrWhiteSpace(_options.BpfFilter))
                    device.Filter = _options.BpfFilter;

                device.OnPacketArrival += OnPacketArrival;
                device.StartCapture();
                _openedDevices.Add(device);
                logger.LogInformation(
                    "WireFish capture started on {Device} (filter: {Filter})",
                    device.Description,
                    device.Filter ?? "(none)");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to start capture on device {Device}", device.Description);
                if (!_options.AllowNoCaptureDevices)
                    throw;
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var device in _openedDevices)
        {
            device.OnPacketArrival -= OnPacketArrival;
            if (device.Started)
            {
                device.StopCapture();
                device.Close();
            }

            logger.LogDebug("Stopped WireFish capture on {Device}", device.Description);
        }

        _openedDevices.Clear();
        return Task.CompletedTask;
    }

    private IEnumerable<LibPcapLiveDevice> SelectDevices()
    {
        var devices = CaptureDeviceList.Instance.OfType<LibPcapLiveDevice>();
        if (_options.CaptureAllDevices)
            return devices;

        var names = _options.DeviceNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return devices.Where(d =>
            names.Contains(d.Name) ||
            names.Contains(d.Description) ||
            (d.Interface?.FriendlyName is { } friendly && names.Contains(friendly)));
    }

    private void OnPacketArrival(object sender, PacketCapture e)
    {
        if (sender is not LibPcapLiveDevice device)
            return;

        try
        {
            var rawPacket = e.GetPacket();
            var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
            _ = packetHandler.EnqueueAsync(device, packet);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to parse or enqueue packet from {Device}", device.Description);
        }
    }
}
