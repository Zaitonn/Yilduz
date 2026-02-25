namespace Yilduz.Network.XMLHttpRequest;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/readyState
/// </summary>
public enum XMLHttpRequestReadyState
{
    Unsent = 0,

    Opened = 1,

    HeadersReceived = 2,

    Loading = 3,

    Done = 4,
}
