using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Yilduz.Network.Fetch;
using Yilduz.Network.XMLHttpRequestEventTarget;
using Yilduz.Network.XMLHttpRequestUpload;
using Yilduz.URLs.URL;
using Yilduz.Utils;

namespace Yilduz.Network.XMLHttpRequest;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest
/// </summary>
public sealed class XMLHttpRequestInstance : XMLHttpRequestEventTargetInstance
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/readyState
    /// </summary>
    public JsValue OnReadyStateChange { get; set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/readyState
    /// </summary>
    public XMLHttpRequestReadyState ReadyState { get; private set; } =
        XMLHttpRequestReadyState.Unsent;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/response
    /// </summary>
    public JsValue Response { get; private set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/responseTextF
    /// </summary>
    public string ResponseText { get; private set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/responseType
    /// </summary>
    public string ResponseType { get; set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/responseURL
    /// </summary>
    public string ResponseURL { get; private set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/responseXML
    /// </summary>
    public JsValue ResponseXML { get; private set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/status
    /// </summary>
    public ushort Status { get; private set; } = 0;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/statusText
    /// </summary>
    public string StatusText { get; private set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/timeout
    /// </summary>
    public long Timeout
    {
        get => _timeout;
        set
        {
            if (_synchronousFlag)
            {
                DOMExceptionHelper
                    .CreateInvalidAccessError(Engine, "Synchronous requests cannot have a timeout")
                    .Throw();
            }
            _timeout = value;
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/withCredentials
    /// </summary>
    public bool WithCredentials
    {
        get => _crossOriginCredentials;
        set
        {
            if (
                ReadyState != XMLHttpRequestReadyState.Unsent
                    && ReadyState != XMLHttpRequestReadyState.Opened
                || _sendFlag
            )
            {
                DOMExceptionHelper.CreateInvalidStateError(Engine, "Request already sent").Throw();
            }

            _crossOriginCredentials = value;
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/upload
    /// </summary>
    public XMLHttpRequestUploadInstance Upload { get; }

    private readonly FetchController _fetchController;
    private bool _sendFlag;

    // private bool _uploadListenerFlag;
    // private bool _timedOutFlag;
    private bool _synchronousFlag;
    private string? _requestMethod;
    private URLInstance? _url;
    private JsValue? _response;
    private JsValue? _responseObject;
    private long _timeout;
    private bool _crossOriginCredentials;
    private readonly List<byte> _receivedBytes;
    private readonly Dictionary<string, string> _authorRequestHeaders;

    internal XMLHttpRequestInstance(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine)
    {
        _fetchController = new(engine);
        _receivedBytes = [];
        _authorRequestHeaders = [];

        Upload = _webApiIntrinsics.XMLHttpRequestUpload.Construct();
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/open
    /// </summary>
    public void Open(
        string method,
        string url,
        bool async = true,
        string? username = null,
        string? password = null
    )
    {
        if (
            string.IsNullOrEmpty(method)
#if !NETSTANDARD2_0
            || method.Contains('\x20')
#else
            || method.Contains("\x20")
#endif
            || method.Equals("CONNECT", StringComparison.OrdinalIgnoreCase)
            || method.Equals("TRACE", StringComparison.OrdinalIgnoreCase)
            || method.Equals("TRACK", StringComparison.OrdinalIgnoreCase)
        )
        {
            DOMExceptionHelper.CreateSecurityError(Engine, "Invalid method").Throw();
        }

        method = method.ToUpperInvariant();

        var parsedUrl = _webApiIntrinsics.URL.Parse(url, null);

        if (parsedUrl.IsNull())
        {
            DOMExceptionHelper.CreateSyntaxError(Engine, "Failed to parse URL: " + url).Throw();
        }

        if (async)
        {
            password = null;
            username = null;
        }

        if (!string.IsNullOrEmpty(parsedUrl.Host))
        {
            if (password is not null)
            {
                parsedUrl.Password = password;
            }
            if (username is not null)
            {
                parsedUrl.Username = username;
            }
        }

        if (Timeout != 0 && !async && string.IsNullOrEmpty(ResponseType))
        {
            DOMExceptionHelper
                .CreateInvalidAccessError(Engine, "Synchronous requests cannot have a timeout")
                .Throw();
        }

        _fetchController.Terminate();

        _sendFlag = default;
        // _uploadListenerFlag = default;
        _requestMethod = method;
        _synchronousFlag = !async;
        _url = parsedUrl;
        _authorRequestHeaders.Clear();
        _response = DOMExceptionHelper.CreateNetworkError(Engine, "Request not sent");
        _responseObject = Null;

        if (ReadyState != XMLHttpRequestReadyState.Opened)
        {
            ReadyState = XMLHttpRequestReadyState.Opened;
            DispatchEvent(
                _webApiIntrinsics.Event.ConstructWithEventName("readystatechange", Undefined)
            );
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/setRequestHeader
    /// </summary>
    public void SetRequestHeader(string name, string value)
    {
        if (ReadyState != XMLHttpRequestReadyState.Opened)
        {
            DOMExceptionHelper.CreateInvalidStateError(Engine, "Request not opened").Throw();
        }
        if (_sendFlag)
        {
            DOMExceptionHelper.CreateInvalidStateError(Engine, "Request already sent").Throw();
        }

        value = value.Trim();

        if (!HttpHelper.IsHeaderValue(value))
        {
            DOMExceptionHelper.CreateSyntaxError(Engine, "Invalid header value").Throw();
        }

        if (HttpHelper.IsForbiddenRequestHeader(name, value))
        {
            return;
        }

        _authorRequestHeaders[name] = value;
    }
}
