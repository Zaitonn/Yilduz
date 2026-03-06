namespace Yilduz.Network.WebSocket;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/WebSocket/readyState
/// </summary>
public enum WebSocketReadyState : ushort
{
    CONNECTING = 0,

    OPEN = 1,

    CLOSING = 2,

    CLOSED = 3,
}
