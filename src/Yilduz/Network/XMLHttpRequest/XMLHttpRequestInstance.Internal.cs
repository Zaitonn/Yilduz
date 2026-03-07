using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Jint.Native;
using Jint.Native.Json;
using Jint.Runtime;
using Yilduz.Extensions;
using Yilduz.Network.Body;
using Yilduz.Network.Fetch;
using Yilduz.Network.Headers;
using Yilduz.Network.Response;
using Yilduz.URLs.URL;
using Yilduz.Utils;

namespace Yilduz.Network.XMLHttpRequest;

public sealed partial class XMLHttpRequestInstance
{
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
    /// </summary>
    private ResponseConcept? _xhrResponse;

    internal void SetResponseHeaders(HeaderList headers)
    {
        _responseHeaders.Clear();
        _responseHeaders.AddRange(headers);
    }

    /// <summary>
    /// https://xhr.spec.whatwg.org/#the-send()-method step 12.11
    /// processResponse algorithm - runs on the JS event-loop thread.
    /// </summary>
    private void ProcessResponse(ResponseConcept response)
    {
        _activeRequestCts?.Dispose();
        _activeRequestCts = null;

        // Step 12.11.1. Set this's response to response.
        _xhrResponse = response;

        // Step 12.11.2. Handle errors for this.
        HandleErrors();

        // Step 12.11.3. If this's response is a network error, then return.
        if (!_sendFlag)
        {
            return;
        }

        // Step 12.11.4. Set this's state to headers received.
        Status = response.Status;
        StatusText = response.StatusMessage;
        ResponseURL = response.Url?.Href ?? string.Empty;
        SetResponseHeaders(response.HeaderList);

        // Step 12.11.5. Fire readystatechange (done by TransitionReadyState).
        TransitionReadyState(XMLHttpRequestReadyState.HEADERS_RECEIVED);

        // Step 12.11.6. If this's state is not headers received, then return.
        if (ReadyState != XMLHttpRequestReadyState.HEADERS_RECEIVED)
        {
            return;
        }

        // Step 12.11.7. If this's response's body is null, run handle response end-of-body and return.
        if (response.Body is null)
        {
            HandleResponseEndOfBody();
            return;
        }

        // Step 12.11.8-9. Let length be Content-Length or 0.
        var contentLength = ExtractContentLength();

        // Step 12.11.10-11. processBodyChunk: append bytes, transition to loading, fire progress.
        // The body is fully buffered in BodyConcept.Source; simulate a single chunk.
        var bytes = response.Body.Source?.TryAsBytes() ?? [];
        if (bytes.Length > 0)
        {
            _receivedBytes.AddRange(bytes);
        }

        // Transition to Loading on first (and only) chunk.
        if (ReadyState == XMLHttpRequestReadyState.HEADERS_RECEIVED)
        {
            TransitionReadyState(XMLHttpRequestReadyState.LOADING);
        }

        FireProgressEvent("progress", (ulong)_receivedBytes.Count, (ulong)contentLength);

        // Step 12.11.12. processEndOfBody -> handle response end-of-body.
        HandleResponseEndOfBody();
    }

    /// <summary>
    /// https://xhr.spec.whatwg.org/#handle-response-end-of-body
    /// </summary>
    private void HandleResponseEndOfBody()
    {
        // Step 1. Handle errors for xhr.
        HandleErrors();

        // Step 2. If xhr's response is a network error, then return.
        if (!_sendFlag)
        {
            return;
        }

        var transmitted = (ulong)_receivedBytes.Count;
        var length = (ulong)ExtractContentLength();

        // Step 6. If synchronous flag is unset, fire progress.
        if (!_synchronousFlag)
        {
            FireProgressEvent("progress", transmitted, length);
        }

        // Build the JS-facing response object before transitioning to Done.
        BuildResponseObject();

        // Step 7. Set state to done.
        // Step 8. Unset send() flag.
        _sendFlag = false;

        // Step 9. Fire readystatechange (done by TransitionReadyState).
        TransitionReadyState(XMLHttpRequestReadyState.DONE);

        // Step 10. Fire a progress event named load.
        FireProgressEvent("load", transmitted, length);

        // Step 11. Fire a progress event named loadend.
        FireProgressEvent("loadend", transmitted, length);
    }

    /// <summary>
    /// https://xhr.spec.whatwg.org/#handle-errors
    /// </summary>
    private void HandleErrors()
    {
        // Step 1. If send() flag is unset, return.
        if (!_sendFlag)
        {
            return;
        }

        // Step 2. If timed out flag is set.
        if (_timedOutFlag)
        {
            RequestErrorSteps("timeout", null);
            return;
        }

        // Step 3. If response's aborted flag is set.
        if (_xhrResponse?.AbortedFlag == true)
        {
            RequestErrorSteps("abort", null);
            return;
        }

        // Step 4. If response is a network error.
        if (_xhrResponse?.Type == Network.Response.ResponseType.Error)
        {
            RequestErrorSteps("error", null);
        }
    }

    private void TransitionReadyState(XMLHttpRequestReadyState state)
    {
        if (ReadyState == state)
        {
            return;
        }

        ReadyState = state;
        FireEvent("readystatechange");
    }

    /// <summary>
    /// https://xhr.spec.whatwg.org/#request-error-steps
    /// </summary>
    private void RequestErrorSteps(string eventName, JsError? jsError)
    {
        // Step 1. Set xhr's state to done.
        ReadyState = XMLHttpRequestReadyState.DONE;

        // Step 2. Unset xhr's send() flag.
        _sendFlag = false;

        // Step 3. Set xhr's response to a network error.
        ResetResponse();

        // Step 4. If xhr's synchronous flag is set, then throw exception.
        if (_synchronousFlag)
        {
            if (jsError is not null)
            {
                throw new JavaScriptException(jsError);
            }
            else
            {
                throw new JavaScriptException(
                    Engine.Intrinsics.Error.Construct("Network error occurred")
                );
            }
        }

        // Step 5. Fire an event named readystatechange at xhr.
        FireEvent("readystatechange");

        // Step 6. If xhr's upload complete flag is unset, then:
        if (!_uploadCompleteFlag)
        {
            _uploadCompleteFlag = true;

            // Step 6.2. If xhr's upload listener flag is set, fire upload events.
            if (_uploadListenerFlag)
            {
                Upload.FireEvent(eventName, 0, 0);
                Upload.FireEvent("loadend", 0, 0);
            }
        }

        // Step 7. Fire a progress event named eventName at xhr with 0 and 0.
        FireProgressEvent(eventName, 0, 0);

        // Step 8. Fire a progress event named loadend at xhr with 0 and 0.
        FireProgressEvent("loadend", 0, 0);
    }

    [MemberNotNull(nameof(Response))]
    private void ResetResponse()
    {
        Response = DOMExceptionHelper.CreateNetworkError(Engine, "Request not sent");
    }

    /// <summary>
    /// Builds the JS-facing Response and ResponseText properties
    /// from the buffered _receivedBytes based on ResponseType.
    /// </summary>
    [MemberNotNull(nameof(Response))]
    private void BuildResponseObject()
    {
        var bytes = _receivedBytes.ToArray();
        var effectiveMimeType =
            _overrideMimeType
            ?? _responseHeaders
                .FirstOrDefault(h =>
                    h.Name.Equals("content-type", StringComparison.OrdinalIgnoreCase)
                )
                ?.Value
            ?? string.Empty;

        var responseType = ResponseType;

        if (responseType is "" or "text")
        {
            var rawText =
                bytes.Length == 0 ? string.Empty : System.Text.Encoding.UTF8.GetString(bytes);
            ResponseText = rawText;
            Response = rawText;
        }
        else if (responseType == "arraybuffer")
        {
            ResponseText = string.Empty;
            Response = Engine.Intrinsics.ArrayBuffer.Construct(bytes);
        }
        else if (responseType == "blob")
        {
            ResponseText = string.Empty;
            Response = _webApiIntrinsics.Blob.CreateInstance(bytes, effectiveMimeType);
        }
        else if (responseType == "json")
        {
            ResponseText = string.Empty;
            try
            {
                Response = new JsonParser(Engine).Parse(System.Text.Encoding.UTF8.GetString(bytes));
            }
            catch
            {
                Response = Null;
            }
        }
        else
        {
            throw new InvalidOperationException($"Unsupported responseType '{responseType}'.");
        }
    }

    /// <summary>
    /// Extracts the Content-Length from the response headers, returning 0 when absent or invalid.
    /// https://fetch.spec.whatwg.org/#header-list-extract-a-length
    /// </summary>
    private long ExtractContentLength()
    {
        var headerValue = _responseHeaders
            .FirstOrDefault(h =>
                h.Name.Equals("content-length", StringComparison.OrdinalIgnoreCase)
            )
            ?.Value;
        return long.TryParse(headerValue, out var cl) ? cl : 0L;
    }

    /// <summary>
    /// Fires a ProgressEvent at this XHR object with the given transmitted/total values.
    /// </summary>
    private void FireProgressEvent(string type, ulong transmitted, ulong length)
    {
        DispatchEvent(
            _webApiIntrinsics.ProgressEvent.CreateInstance(type, transmitted, length, true)
        );
    }
}
