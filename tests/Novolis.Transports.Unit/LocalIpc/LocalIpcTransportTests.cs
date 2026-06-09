using Novolis.Transports.LocalIpc;

namespace Novolis.Transports.Unit.LocalIpc;

public sealed class LocalIpcTransportTests
{
    [Test]
    public async Task NamedPipe_listener_accepts_while_prior_connection_is_open()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var pipeName = $"novolis-test-{Guid.NewGuid():N}";
        var endpoint = new LocalIpcEndpoint(pipeName, LocalIpcTransportKind.NamedPipe);
        await using var listener = LocalIpcTransport.CreateListener(endpoint);

        var firstAcceptTask = listener.AcceptAsync();
        await using var firstClientConnection = await LocalIpcTransport.CreateClient().ConnectAsync(endpoint);
        await using var firstServerConnection = await firstAcceptTask;

        var secondAcceptTask = listener.AcceptAsync();
        await using var secondClientConnection = await LocalIpcTransport.CreateClient().ConnectAsync(endpoint);
        await using var secondServerConnection = await secondAcceptTask;
    }
}
