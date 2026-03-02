using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Yilduz.Network.Body;
using Yilduz.Network.Fetch;
using Yilduz.Network.Headers;
using Yilduz.Network.Request;
using Yilduz.Network.Response;
using Yilduz.Network.XMLHttpRequestEventTarget;
using Yilduz.Network.XMLHttpRequestUpload;
using Yilduz.URLs.URL;
using Yilduz.Utils;

namespace Yilduz.Network.XMLHttpRequest;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest
/// </summary>
public sealed partial class XMLHttpRequestInstance : XMLHttpRequestEventTargetInstance
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
    public JsValue Response { get; private set; }

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
    /// <br/>
    /// https://xhr.spec.whatwg.org/#the-timeout-attribute
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
    /// <br/>
    /// https://xhr.spec.whatwg.org/#the-withcredentials-attribute
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

    private FetchController _fetchController;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#send-flag
    /// </summary>
    private bool _sendFlag;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#upload-listener-flag
    /// </summary>
    private bool _uploadListenerFlag;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#timed-out-flag
    /// </summary>
    private bool _timedOutFlag;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#upload-complete-flag
    /// </summary>
    private bool _uploadCompleteFlag;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#synchronous-flag
    /// </summary>
    private bool _synchronousFlag;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#request-method
    /// </summary>
    private string? _requestMethod;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#request-url
    /// </summary>
    private URLInstance? _requestUrl;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#response-object
    /// </summary>
    private JsValue _responseObject = Null;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#override-mime-type
    /// </summary>
    private string? _overrideMimeType;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#timeout
    /// </summary>
    private long _timeout;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#cross-origin-credentials
    /// </summary>
    private bool _crossOriginCredentials;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#received-bytes
    /// </summary>
    private readonly List<byte> _receivedBytes = [];

    /// <summary>
    /// https://xhr.spec.whatwg.org/#author-request-headers
    /// </summary>
    private readonly HeaderList _authorRequestHeaders = [];

    private readonly HeaderList _responseHeaders = [];
    private CancellationTokenSource? _activeRequestCts;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#request-body
    /// </summary>
    private BodyConcept? _requestBody;

    /// <summary>
    /// https://xhr.spec.whatwg.org/#response
    /// The response concept received from the fetch, used by handle-errors.
    /// </summary>
    private ResponseConcept? _xhrResponse;

    internal XMLHttpRequestInstance(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, webApiIntrinsics)
    {
        _fetchController = new(engine);
        ResetResponse();

        // Set this’s upload object to a new XMLHttpRequestUpload object.
        Upload = _webApiIntrinsics.XMLHttpRequestUpload.CreateInstance();
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/open
    /// <br/>
    /// https://xhr.spec.whatwg.org/#the-open()-method
    /// </summary>
    public void Open(
        string method,
        string url,
        bool async = true,
        string? username = null,
        string? password = null
    )
    {
        // If this’s relevant global object is a Window object and its associated Document is not fully active, then throw an "InvalidStateError" DOMException.

        // If method is not a method, then throw a "SyntaxError" DOMException.
        if (!HttpHelper.IsMethod(method))
        {
            DOMExceptionHelper.CreateSyntaxError(Engine, "Invalid method").Throw();
        }

        // If method is a forbidden method, then throw a "SecurityError" DOMException.
        if (HttpHelper.IsForbiddenMethod(method))
        {
            DOMExceptionHelper.CreateSecurityError(Engine, "Invalid method").Throw();
        }

        // Normalize method.
        method = HttpHelper.NormalizeMethod(method);

        // Let parsedURL be the result of encoding-parsing a URL url, relative to this’s relevant settings object.
        var parsedUrl = _webApiIntrinsics.URL.TryParse(url, _webApiIntrinsics.Options.BaseUrl);

        // If parsedURL is failure, then throw a "SyntaxError" DOMException.
        if (parsedUrl is null)
        {
            DOMExceptionHelper.CreateSyntaxError(Engine, "Failed to parse URL: " + url).Throw();
        }

        // If the async argument is omitted, set async to true, and set username and password to null.
        if (async)
        {
            password = null;
            username = null;
        }

        // If parsedURL’s host is non-null, then
        if (!string.IsNullOrEmpty(parsedUrl.Host))
        {
            // If the username argument is not null, set the username given parsedURL and username.
            if (username is not null)
            {
                parsedUrl.Username = username;
            }

            // If the password argument is not null, set the password given parsedURL and password
            if (password is not null)
            {
                parsedUrl.Password = password;
            }
        }

        // If async is false, the current global object is a Window object,
        //  and either this’s timeout is not 0 or this’s response type is not the empty string,
        //  then throw an "InvalidAccessError" DOMException.
        if (Timeout != 0 && !async && string.IsNullOrEmpty(ResponseType))
        {
            DOMExceptionHelper
                .CreateInvalidAccessError(Engine, "Synchronous requests cannot have a timeout")
                .Throw();
        }

        // Terminate this’s fetch controller.
        _fetchController.Terminate();

        // Set variables associated with the object as follows:
        // Unset this’s send() flag.
        _sendFlag = false;

        // Unset this’s upload listener flag.
        _uploadListenerFlag = false;

        // Set this’s request method to method.
        _requestMethod = method;

        // Set this’s request URL to parsedURL.
        _requestUrl = parsedUrl;

        // Set this’s synchronous flag if async is false; otherwise unset this’s synchronous flag.
        _synchronousFlag = !async;

        // Empty this’s author request headers.
        _authorRequestHeaders.Clear();

        // Set this’s response to a network error.
        ResetResponse();

        // Set this’s received bytes to the empty byte sequence.
        _receivedBytes.Clear();
        _responseHeaders.Clear();

        // Set this’s response object to null.
        _responseObject = Null;

        _activeRequestCts?.Dispose();
        _activeRequestCts = null;

        // If this’s state is not opened, then
        // Set this’s state to opened.
        // Fire an event named readystatechange at this.
        TransitionReadyState(XMLHttpRequestReadyState.Opened);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/setRequestHeader
    /// <br/>
    /// https://xhr.spec.whatwg.org/#the-setrequestheader()-method
    /// </summary>
    public void SetRequestHeader(string name, string value)
    {
        // If this’s state is not opened, then throw an "InvalidStateError" DOMException.
        if (ReadyState != XMLHttpRequestReadyState.Opened)
        {
            DOMExceptionHelper.CreateInvalidStateError(Engine, "Request not opened").Throw();
        }

        // If this’s send() flag is set, then throw an "InvalidStateError" DOMException.
        if (_sendFlag)
        {
            DOMExceptionHelper.CreateInvalidStateError(Engine, "Request already sent").Throw();
        }

        // Normalize value.
        value = HttpHelper.Normalize(value);

        // If name is not a header name or value is not a header value, then throw a "SyntaxError" DOMException.
        if (!HttpHelper.IsHeaderName(name))
        {
            DOMExceptionHelper.CreateSyntaxError(Engine, "Invalid header name").Throw();
        }
        if (!HttpHelper.IsHeaderValue(value))
        {
            DOMExceptionHelper.CreateSyntaxError(Engine, "Invalid header value").Throw();
        }

        // If (name, value) is a forbidden request-header, then return.
        if (HttpHelper.IsForbiddenRequestHeader(name, value))
        {
            return;
        }

        // Combine (name, value) in this’s author request headers.
        var first = _authorRequestHeaders.FirstOrDefault(entry => entry.Name == name);
        if (first is null)
        {
            _authorRequestHeaders.Add(new(name, value));
        }
        else
        {
            _authorRequestHeaders.RemoveAll(entry => entry.Name == name);
            _authorRequestHeaders.Add(new(name, first.Value + ", " + value));
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/overrideMimeType
    /// </summary>
    public void OverrideMimeType(string mimeType)
    {
        if (
            ReadyState == XMLHttpRequestReadyState.Loading
            || ReadyState == XMLHttpRequestReadyState.Done
            || _sendFlag
        )
        {
            DOMExceptionHelper
                .CreateInvalidStateError(Engine, "Cannot override mime type after sending")
                .Throw();
        }

        mimeType = mimeType?.Trim() ?? string.Empty;
        if (mimeType.Length == 0)
        {
            DOMExceptionHelper.CreateSyntaxError(Engine, "Invalid MIME type").Throw();
        }

        _overrideMimeType = mimeType;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/getResponseHeader
    /// </summary>
    public string? GetResponseHeader(string name)
    {
        if (!HttpHelper.IsHeaderName(name))
        {
            TypeErrorHelper.Throw(Engine, "Invalid header name", nameof(GetResponseHeader));
        }

        if (
            ReadyState == XMLHttpRequestReadyState.Unsent
            || ReadyState == XMLHttpRequestReadyState.Opened
        )
        {
            return null;
        }

        var values = _responseHeaders
            .Where(header => header.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .Where(header => !HttpHelper.IsForbiddenResponseHeader(header.Name))
            .Select(header => header.Value)
            .ToArray();

        return values.Length == 0 ? null : string.Join(", ", values);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/getAllResponseHeaders
    /// </summary>
    public string GetAllResponseHeaders()
    {
        if (
            ReadyState == XMLHttpRequestReadyState.Unsent
            || ReadyState == XMLHttpRequestReadyState.Opened
        )
        {
            return string.Empty;
        }

        var combinedHeaders = _responseHeaders
            .Where(header => !HttpHelper.IsForbiddenResponseHeader(header.Name))
            .GroupBy(header => header.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
                (
                    Name: group.Key.ToLowerInvariant(),
                    Value: string.Join(", ", group.Select(header => header.Value))
                )
            )
            .OrderBy(header => header.Name, StringComparer.Ordinal);

        return string.Join(
            "\n",
            combinedHeaders.Select(header => $"{header.Name}: {header.Value}")
        );
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/send
    /// <br/>
    /// https://xhr.spec.whatwg.org/#the-send()-method
    /// </summary>
    public void Send(JsValue? body = null)
    {
        // If this’s state is not opened, then throw an "InvalidStateError" DOMException.
        if (
            ReadyState != XMLHttpRequestReadyState.Opened
            || _requestMethod is null
            || _requestUrl is null
        )
        {
            DOMExceptionHelper.CreateInvalidStateError(Engine, "Request not opened").Throw();
        }

        // If this’s send() flag is set, then throw an "InvalidStateError" DOMException.
        if (_sendFlag)
        {
            DOMExceptionHelper.CreateInvalidStateError(Engine, "Request already sent").Throw();
        }

        _sendFlag = true;

        // If this’s request method is `GET` or `HEAD`, then set body to null.
        if (
            _requestMethod.Equals("GET", StringComparison.OrdinalIgnoreCase)
            || _requestMethod.Equals("HEAD", StringComparison.OrdinalIgnoreCase)
        )
        {
            body = null;
        }

        // If body is not null, then
        if (body is not null && !body.IsNull() && !body.IsUndefined())
        {
            // Let extractedContentType be null.
            string? extractedContentType = null;

            // If body is a Document, then set this’s request body to body, serialized, converted, and UTF-8 encoded.
            // TODO
            // if (body is DocumentInstance document)
            { }
            // else
            // Otherwise
            {
                // Let bodyWithType be the result of safely extracting body.
                var bodyWithType = BodyExtractor.Extract(Engine, body);

                // Set this’s request body to bodyWithType’s body.
                _requestBody = bodyWithType.Body;
                extractedContentType = bodyWithType.Type;
            }

            // Let originalAuthorContentType be the result of getting `Content-Type` from this’s author request headers.
            var originalAuthorContentType = _authorRequestHeaders.Get("Content-Type");

            // If originalAuthorContentType is non-null, then:
            if (originalAuthorContentType is not null)
            {
                // If body is a Document or a USVString, then:
                // TODO
                if (body.IsString())
                {
                    try
                    {
                        // Let contentTypeRecord be the result of parsing originalAuthorContentType
                        var contentTypeRecord = MIMETypeHelper.Parse(originalAuthorContentType);

                        // If contentTypeRecord is not failure, contentTypeRecord’s parameters["charset"] exists,
                        // and parameters["charset"] is not an ASCII case-insensitive match for "UTF-8", then:
                        if (
                            contentTypeRecord.Parameters.TryGetValue(
                                "charset",
                                out var charsetValue
                            ) && !charsetValue.Equals("UTF-8", StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            // Set contentTypeRecord’s parameters["charset"] to "UTF-8".
                            contentTypeRecord.Parameters["charset"] = "UTF-8";

                            // Let newContentTypeSerialized be the result of serializing contentTypeRecord.
                            var newContentTypeSerialized = MIMETypeHelper.Serialize(
                                contentTypeRecord
                            );

                            // Set (`Content-Type`, newContentTypeSerialized) in this’s author request headers
                            _authorRequestHeaders.Set("Content-Type", newContentTypeSerialized);
                        }
                    }
                    catch { }
                }
            }
            // Otherwise:
            else
            {
                // If body is an HTML document, then set (`Content-Type`, `text/html;charset=UTF-8`) in this’s author request headers.
                // Otherwise, if body is an XML document, set (`Content-Type`, `application/xml;charset=UTF-8`) in this’s author request headers.
                // Otherwise, if extractedContentType is not null, set (`Content-Type`, extractedContentType) in this’s author request headers.
                if (extractedContentType is not null)
                {
                    _authorRequestHeaders.Set("Content-Type", extractedContentType);
                }
            }
        }

        // If one or more event listeners are registered on this’s upload object, then set this’s upload listener flag.
        if (
            Upload.HasEventListeners()
            || Upload.OnAbort is Function
            || Upload.OnError is Function
            || Upload.OnLoad is Function
            || Upload.OnLoadStart is Function
            || Upload.OnProgress is Function
            || Upload.OnTimeout is Function
            || Upload.OnLoadEnd is Function
        )
        {
            _uploadListenerFlag = true;
        }

        //Let req be a new request, initialized as follows:
        var req = new RequestConcept
        {
            Method = _requestMethod,
            URLList = [_requestUrl],
            HeaderList = _authorRequestHeaders,
            UnsafeRequestFlag = true,
            Body = _requestBody,
            Client = _webApiIntrinsics.Options,
            CredentialsMode = _crossOriginCredentials
                ? Credentials.Include
                : Credentials.SameOrigin,
            Mode = Mode.CORS,
            UseCORSPreflightFlag = true,
            InitiatorType = InitiatorType.XmlHttpRequest,
            UseURLCredentialsFlag =
                !string.IsNullOrEmpty(_requestUrl.Username)
                || !string.IsNullOrEmpty(_requestUrl.Password),
        };

        // Unset this’s upload complete flag.
        _uploadCompleteFlag = false;

        // Unset this’s timed out flag.
        _timedOutFlag = false;

        // If req’s body is null, then set this’s upload complete flag.
        if (req.Body is null)
        {
            _uploadCompleteFlag = true;
        }

        // Set this’s send() flag.
        _sendFlag = true;

        _activeRequestCts?.Dispose();
        _activeRequestCts = CancellationTokenSource.CreateLinkedTokenSource(
            _webApiIntrinsics.Options.CancellationToken
        );

        // Step 12. If this's synchronous flag is unset, then:
        if (!_synchronousFlag)
        {
            // Step 12.1. Fire a progress event named loadstart at this with 0 and 0.
            FireProgressEvent("loadstart", 0, 0);

            // Step 12.2-3. requestBodyTransmitted = 0; requestBodyLength = body's length or 0.
            var requestBodyTransmitted = 0L;
            var requestBodyLength = req.Body?.Length ?? 0L;

            // Step 12.5. If upload complete flag is unset and upload listener flag is set,
            // fire loadstart at upload object.
            if (!_uploadCompleteFlag && _uploadListenerFlag)
            {
                Upload.FireEvent(
                    "loadstart",
                    (ulong)requestBodyTransmitted,
                    (ulong)requestBodyLength
                );
            }

            // Step 12.6. If state is not opened or send() flag is unset, return.
            if (ReadyState != XMLHttpRequestReadyState.Opened || !_sendFlag)
            {
                return;
            }

            // Step 12.7. processRequestBodyChunkLength.
            // These callbacks fire from the background upload thread, so they are
            // dispatched through the event loop to stay on the JS thread.
            var lastUploadProgressTime = DateTimeOffset.MinValue;
            Action<long> processRequestBodyChunkLength = bytesLength =>
            {
                _webApiIntrinsics.EventLoop.QueueMacrotask(() =>
                {
                    requestBodyTransmitted += bytesLength;
                    var now = DateTimeOffset.UtcNow;
                    // Throttle: fire at most once per ~50 ms.
                    if ((now - lastUploadProgressTime).TotalMilliseconds < 50)
                    {
                        return;
                    }
                    lastUploadProgressTime = now;
                    if (_uploadListenerFlag)
                    {
                        Upload.FireEvent(
                            "progress",
                            (ulong)requestBodyTransmitted,
                            (ulong)requestBodyLength
                        );
                    }
                });
            };

            // Step 12.9. processRequestEndOfBody.
            Action processRequestEndOfBody = () =>
            {
                _webApiIntrinsics.EventLoop.QueueMacrotask(() =>
                {
                    // Step 12.9.1. Set upload complete flag.
                    _uploadCompleteFlag = true;

                    // Step 12.9.2. If upload listener flag is unset, return.
                    if (!_uploadListenerFlag)
                    {
                        return;
                    }

                    // Steps 12.9.3-5.
                    Upload.FireEvent(
                        "progress",
                        (ulong)requestBodyTransmitted,
                        (ulong)requestBodyLength
                    );
                    Upload.FireEvent(
                        "load",
                        (ulong)requestBodyTransmitted,
                        (ulong)requestBodyLength
                    );
                    Upload.FireEvent(
                        "loadend",
                        (ulong)requestBodyTransmitted,
                        (ulong)requestBodyLength
                    );
                });
            };

            // Step 12.13. Set fetch controller.
            _fetchController = FetchImplementation.Fetch(
                Engine,
                _webApiIntrinsics,
                req,
                processRequestBodyChunkLength: processRequestBodyChunkLength,
                processRequestEndOfBody: processRequestEndOfBody,
                processResponse: ProcessResponse,
                processResponseEndOfBody: null,
                cancellationToken: _activeRequestCts.Token
            );

            // Step 12.15. Timeout: run in parallel.
            if (_timeout > 0)
            {
                var timeoutMs = (int)_timeout;
                var cts = _activeRequestCts;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(timeoutMs, cts.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // Request completed before timeout – nothing to do.
                        return;
                    }

                    _timedOutFlag = true;
                    _fetchController.Terminate();
                    cts.Cancel();
                });
            }
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/abort
    /// <br/>
    /// https://xhr.spec.whatwg.org/#the-abort()-method
    /// </summary>
    public void Abort()
    {
        _activeRequestCts?.Cancel();

        // Abort this’s fetch controller.
        _fetchController.Abort(Null);

        // If this’s state is opened with this’s send() flag set, headers received, or loading,
        // then run the request error steps for this and abort.
        if (
            ReadyState == XMLHttpRequestReadyState.Opened && _sendFlag
            || ReadyState == XMLHttpRequestReadyState.Headers_Received
            || ReadyState == XMLHttpRequestReadyState.Loading
        )
        {
            RequestErrorSteps("abort", null);
        }

        // If this’s state is done, then set this’s state to unsent and this’s response to a network error.
        if (ReadyState == XMLHttpRequestReadyState.Done)
        {
            ReadyState = XMLHttpRequestReadyState.Unsent;
            ResetResponse();
        }
    }
}
