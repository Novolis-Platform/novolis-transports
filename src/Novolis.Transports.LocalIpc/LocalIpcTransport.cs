using System.Net;
using System.Net.Sockets;
using System.IO.Pipes;

namespace Novolis.Transports.LocalIpc;

/// <summary>Factory and runtime implementation for local IPC sessions.</summary>
public static class LocalIpcTransport
{
    public static ILocalIpcClient CreateClient() => new DefaultClient();

    public static ILocalIpcListener CreateListener(LocalIpcEndpoint endpoint) => endpoint.Kind switch
    {
        LocalIpcTransportKind.NamedPipe => new NamedPipeListener(endpoint),
        LocalIpcTransportKind.UnixDomainSocket => new UnixDomainSocketListener(endpoint),
        _ => OperatingSystem.IsWindows()
            ? new NamedPipeListener(endpoint with { Kind = LocalIpcTransportKind.NamedPipe })
            : new UnixDomainSocketListener(endpoint with { Kind = LocalIpcTransportKind.UnixDomainSocket }),
    };

    private static LocalIpcTransportKind ResolveKind(LocalIpcEndpoint endpoint) =>
        endpoint.Kind != LocalIpcTransportKind.Auto
            ? endpoint.Kind
            : OperatingSystem.IsWindows()
                ? LocalIpcTransportKind.NamedPipe
                : LocalIpcTransportKind.UnixDomainSocket;

    private sealed class DefaultClient : ILocalIpcClient
    {
        public ValueTask<ILocalIpcConnection> ConnectAsync(LocalIpcEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            endpoint = endpoint with { Kind = ResolveKind(endpoint) };
            return endpoint.Kind switch
            {
                LocalIpcTransportKind.NamedPipe => ConnectNamedPipeAsync(endpoint, cancellationToken),
                LocalIpcTransportKind.UnixDomainSocket => ConnectUnixSocketAsync(endpoint, cancellationToken),
                _ => throw new NotSupportedException($"Unsupported local IPC transport kind: {endpoint.Kind}."),
            };
        }
    }

    private static async ValueTask<ILocalIpcConnection> ConnectNamedPipeAsync(LocalIpcEndpoint endpoint, CancellationToken cancellationToken)
    {
        var client = new NamedPipeClientStream(".", endpoint.Address, PipeDirection.InOut, PipeOptions.Asynchronous);
        await client.ConnectAsync(cancellationToken).ConfigureAwait(false);
        return new LocalIpcConnection(client);
    }

    private static async ValueTask<ILocalIpcConnection> ConnectUnixSocketAsync(LocalIpcEndpoint endpoint, CancellationToken cancellationToken)
    {
        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        await socket.ConnectAsync(new UnixDomainSocketEndPoint(endpoint.Address), cancellationToken).ConfigureAwait(false);
        return new LocalIpcConnection(new NetworkStream(socket, ownsSocket: true));
    }

    private sealed class NamedPipeListener : ILocalIpcListener
    {
        private readonly LocalIpcEndpoint _endpoint;

        public NamedPipeListener(LocalIpcEndpoint endpoint) => _endpoint = endpoint;

        public async ValueTask<ILocalIpcConnection> AcceptAsync(CancellationToken cancellationToken = default)
        {
            var server = new NamedPipeServerStream(
                _endpoint.Address,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            return new LocalIpcConnection(server);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class UnixDomainSocketListener : ILocalIpcListener
    {
        private readonly Socket _listener;

        public UnixDomainSocketListener(LocalIpcEndpoint endpoint)
        {
            if (File.Exists(endpoint.Address))
                File.Delete(endpoint.Address);

            _listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            _listener.Bind(new UnixDomainSocketEndPoint(endpoint.Address));
            _listener.Listen(backlog: 1);
        }

        public async ValueTask<ILocalIpcConnection> AcceptAsync(CancellationToken cancellationToken = default)
        {
            var socket = await _listener.AcceptAsync(cancellationToken).ConfigureAwait(false);
            return new LocalIpcConnection(new NetworkStream(socket, ownsSocket: true));
        }

        public ValueTask DisposeAsync()
        {
            _listener.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
