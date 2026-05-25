using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Novolis.Transports.Tcp.Abstractions;

namespace Novolis.Transports.Tcp.Server;

internal class TcpConnectionHandler : ConnectionHandler
{
	private readonly ILogger<TcpConnectionHandler> _logger;
	private readonly TcpConnectionRequestDelegate _pipeline;

	public TcpConnectionHandler(
		ILogger<TcpConnectionHandler> logger,
		IConnectionHandler connectionHandler,
		IEnumerable<ITcpConnectionMiddleware>? middlewares = null)
	{
		_logger = logger;
		ArgumentNullException.ThrowIfNull(connectionHandler);
		_pipeline = TcpConnectionPipeline.Build(
			input => new ValueTask<ReadOnlyMemory<byte>>(connectionHandler.HandleAsync(input)),
			middlewares);
	}

	public override async Task OnConnectedAsync(ConnectionContext connection)
	{
		_logger.LogDebug("Connected: {ConnectionId}", connection.ConnectionId);

		while (true)
		{
			var result = await connection.Transport.Input.ReadAsync();
			var buffer = result.Buffer;

			foreach (var segment in buffer)
			{
				if (segment.IsEmpty) continue;
				var responseBytes = await _pipeline(segment);
				await connection.Transport.Output.WriteAsync(responseBytes);
			}

			if (result.IsCompleted)
				break;

			connection.Transport.Input.AdvanceTo(buffer.End);
		}

		_logger.LogDebug("Disconnected: {ConnectionId}", connection.ConnectionId);
	}
}