using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
public class Client
{
	public WebSocket _socket { get; set; }
	public TaskCompletionSource<object> _tcs { get; set; }
	public bool _disconnected { get; set; }

	public Client(WebSocket socket, TaskCompletionSource<object> tcs)
	{
		_socket = socket;
		_tcs = tcs;
		_disconnected = false;
	}
}

public class SocketResponse
{
	[JsonProperty("action")]
	public string Action { get; set; }
}

public class WebSocketService : IHostedService, IDisposable
{
	private readonly ILogger<WebSocketService> _logService;
	private Timer? _timer;
	private static int _counter = 0;
	private static List<Client> _clients = new List<Client>();
	private CancellationToken runToken;

	public WebSocketService(ILogger<WebSocketService> logService)
	{
		_logService = logService;
	}

	public static void RegisterClient(Client newClient)
	{
		_clients.Add(newClient);
	}

	private void DisconnectClient(Client client)
	{
		lock (client)
		{
			client._disconnected = true;
		}
	}

	private void RemoveDisconnectedClients()
	{
		lock (_clients)
		{
			_clients.RemoveAll((client) =>
				{
					if (client._disconnected) _logService.LogInformation("Client disconnected");
					return client._disconnected;
				}
			);
		}
	}

	public async void StartPingChecks()
	{
		while (!runToken.IsCancellationRequested)
		{
			_logService.LogInformation("Checking client ping");
			foreach (var client in _clients)
			{
				if (client._socket.State == WebSocketState.CloseReceived)
				{
					DisconnectClient(client);
					continue;
				}
				try
				{
					// Send a ping, expect a pong
					var pingMessage = new { action = "PING" };
					await client._socket.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(pingMessage)), WebSocketMessageType.Text, true, CancellationToken.None);
					// Check if we can receive a message
					var recvBuffer = new Byte[4096];
					var strBuffer = "";
					var endOfMessage = false;
					while (!endOfMessage)
					{
						var timeOut = new CancellationTokenSource(500).Token;
						var res = await client._socket.ReceiveAsync(recvBuffer, timeOut);
						endOfMessage = res.EndOfMessage;
						strBuffer += Encoding.UTF8.GetString(recvBuffer, 0, res.Count);
					}

					var pingResponse = JsonConvert.DeserializeObject<SocketResponse>(strBuffer);
					if (pingResponse?.Action == "PONG")
					{
						continue;
					}
					else
					{
						DisconnectClient(client);
						continue;
					}
				}
				catch
				{
					// Assume connection is broken.
					_logService.LogInformation("Client connection broken, disconnected");
					DisconnectClient(client);
				}
			}
			RemoveDisconnectedClients();
			await Task.Delay(5000);
		}
	}
	public void BroadcastTimerUpdate(object? state)
	{
		_logService.LogInformation("WebSocketService: Timer fired.");
		var payloadObject = new
		{
			action = "COUNTER",
			value = _counter
		};

		var payloadString = JsonConvert.SerializeObject(payloadObject);
		var payloadBytes = Encoding.UTF8.GetBytes(payloadString);

		foreach (var client in _clients)
		{
			try
			{
				client._socket.SendAsync(payloadBytes, WebSocketMessageType.Text, true, CancellationToken.None);
				_logService.LogInformation("Done");
			}
			catch (WebSocketException exception)
			{
				_logService.LogError(exception, "WebSocket send error.");
				if (exception.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
				{
					// Try to close the connection down and remove it from this list
					DisconnectClient(client);
					client._tcs.TrySetResult(new { completed = true });
				}
			}
		}

		RemoveDisconnectedClients();

		_counter++;
	}
	public Task StartAsync(CancellationToken cancellationToken)
	{
		_logService.LogInformation("Registered WebSocketService, will broadcast at 15 second intervals");

		_timer = new Timer(BroadcastTimerUpdate, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
		StartPingChecks();

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_logService.LogInformation("Stopping WebSocketService.");
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		_timer?.Dispose();
		foreach (var client in _clients)
		{
			client._socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closing.", CancellationToken.None);
		}
	}

	public void BroadcastMessage(object payload)
	{
		_logService.LogInformation("WebSocketService: Broadcasting message to clients.");

		var payloadString = JsonConvert.SerializeObject(payload);
		var payloadBytes = Encoding.UTF8.GetBytes(payloadString);

		foreach (var client in _clients)
		{
			try
			{
				client._socket.SendAsync(payloadBytes, WebSocketMessageType.Text, true, CancellationToken.None);
			}
			catch (WebSocketException exception)
			{
				_logService.LogError(exception, "WebSocket send error.");
				if (exception.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
				{
					// Try to close the connection down and remove it from this list
					DisconnectClient(client);
					client._tcs.TrySetResult(new { completed = true });
				}
			}
		}

		RemoveDisconnectedClients();
	}
}