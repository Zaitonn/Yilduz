using System;
using Fleck;

namespace Yilduz.Tests.WebSocket;

public abstract class WebSocketTestBase : TestBase
{
    private readonly WebSocketServer _server;

    protected string WsUrl { get; }

    protected Action<IWebSocketConnection>? _clientConnected;

    protected Action<IWebSocketConnection>? _clientClosed;

    protected Action<IWebSocketConnection, string>? _messageReceived;

    protected Action<IWebSocketConnection, byte[]>? _binaryMessageReceived;

    protected Action<IWebSocketConnection, Exception>? _errorOccurred;

    protected WebSocketTestBase()
    {
        var port = NetworkTestHelper.GetAvailablePort();
        WsUrl = $"ws://localhost:{port}";

        // Suppress Fleck's console log output during tests.
        FleckLog.LogAction = (_, _, _) => { };

        _server = new WebSocketServer($"ws://0.0.0.0:{port}");
        _server.Start(socket =>
        {
            socket.OnOpen = () => _clientConnected?.Invoke(socket);
            socket.OnClose = () => _clientClosed?.Invoke(socket);
            socket.OnMessage = msg => _messageReceived?.Invoke(socket, msg);
            socket.OnBinary = data => _binaryMessageReceived?.Invoke(socket, data);
            socket.OnError = ex => _errorOccurred?.Invoke(socket, ex);
        });
    }

    protected override void OnDisposing()
    {
        _server.Dispose();
    }
}
