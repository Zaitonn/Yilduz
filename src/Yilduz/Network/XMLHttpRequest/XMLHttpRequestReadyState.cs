namespace Yilduz.Network.XMLHttpRequest;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/readyState
/// </summary>
public enum XMLHttpRequestReadyState
{
    UNSENT = 0,

    OPENED = 1,

    HEADERS_RECEIVED = 2,

    LOADING = 3,

    DONE = 4,
}
