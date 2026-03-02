namespace Yilduz.Network.WebSocket;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/WebSocket/readyState
/// </summary>
public enum WebSocketReadyState : ushort
{
    Connecting = 0,

    Open = 1,

    Closing = 2,

    Closed = 3,
}
