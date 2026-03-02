using System.Net.WebSockets;
using System.Threading;
using Jint;
using Jint.Native;
using Yilduz.Data.Blob;
using Yilduz.Events.EventTarget;
using Yilduz.Extensions;
using Yilduz.Utils;
using SystemEncoding = System.Text.Encoding;

namespace Yilduz.Network.WebSocket;

/// <summary>
/// https://websockets.spec.whatwg.org/#the-websocket-interface
/// </summary>
public sealed partial class WebSocketInstance : EventTargetInstance
{
    private readonly string[] _protocols;

    internal WebSocketInstance(
        Engine engine,
        string url,
        string[] protocols,
        WebApiIntrinsics webApiIntrinsics
    )
        : base(engine, webApiIntrinsics)
    {
        Url = url;
        _protocols = protocols;

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _webApiIntrinsics.Options.CancellationToken
        );

        _client = new();

        _cancellationTokenSource.Token.Register(() =>
        {
            if (_client.State == WebSocketState.Connecting || _client.State == WebSocketState.Open)
            {
                _client.Abort();
            }

            _client.Dispose();
            _cancellationTokenSource.Dispose();
        });

        _ = ConnectAsync();
    }

    /// <summary>
    /// https://websockets.spec.whatwg.org/#dom-websocket-url
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// https://websockets.spec.whatwg.org/#dom-websocket-readystate
    /// </summary>
    public WebSocketReadyState ReadyState { get; private set; } = WebSocketReadyState.Connecting;

    /// <summary>
    /// https://websockets.spec.whatwg.org/#dom-websocket-bufferedamount
    /// </summary>
    public ulong BufferedAmount { get; private set; } = 0;

    /// <summary>
    /// https://websockets.spec.whatwg.org/#dom-websocket-extensions
    /// </summary>
    public string? Extensions { get; private set; } = string.Empty;

    /// <summary>
    /// https://websockets.spec.whatwg.org/#dom-websocket-protocol
    /// </summary>
    public string Protocol { get; private set; } = string.Empty;

    private string _binaryType = "blob";

    /// <summary>
    /// https://websockets.spec.whatwg.org/#dom-websocket-binarytype
    /// </summary>
    public string BinaryType
    {
        get => _binaryType;
        set
        {
            if (value is "blob" or "arraybuffer")
            {
                _binaryType = value;
            }
            else
            {
                TypeErrorHelper.Throw(
                    Engine,
                    $"Failed to set the 'binaryType' property on 'WebSocket': The provided value '{value}' is not a valid enum value."
                );
            }
        }
    }

    /// <summary>
    /// https://websockets.spec.whatwg.org/#handler-websocket-onopen
    /// </summary>
    public JsValue OnOpen { get; set; } = Null;

    /// <summary>
    /// https://websockets.spec.whatwg.org/#handler-websocket-onmessage
    /// </summary>
    public JsValue OnMessage { get; set; } = Null;

    /// <summary>
    /// https://websockets.spec.whatwg.org/#handler-websocket-onerror
    /// </summary>
    public JsValue OnError { get; set; } = Null;

    /// <summary>
    /// https://websockets.spec.whatwg.org/#handler-websocket-onclose
    /// </summary>
    public JsValue OnClose { get; set; } = Null;

    /// <summary>
    /// https://websockets.spec.whatwg.org/#dom-websocket-send
    /// </summary>
    public void Send(JsValue data)
    {
        // If this’s ready state is CONNECTING, then throw an "InvalidStateError" DOMException.
        if (ReadyState == WebSocketReadyState.Connecting)
        {
            DOMExceptionHelper
                .CreateInvalidStateError(
                    Engine,
                    "Failed to execute 'send' on 'WebSocket': Still in CONNECTING state."
                )
                .Throw();
        }

        if (ReadyState == WebSocketReadyState.Closing || ReadyState == WebSocketReadyState.Closed)
        {
            return;
        }

        byte[] bytes;
        WebSocketMessageType msgType;

        if (data.TryAsBytes() is { } rawBytes)
        {
            // §9.3 step 6.4: BufferSource (ArrayBuffer / TypedArray / DataView) → raw bytes.
            bytes = rawBytes;
            msgType = WebSocketMessageType.Binary;
        }
        else if (data is BlobInstance blob)
        {
            // §9.3 step 6.5: Blob → read contents.
            bytes = [.. blob.Value];
            msgType = WebSocketMessageType.Binary;
        }
        else
        {
            bytes = SystemEncoding.UTF8.GetBytes(data.ToString());
            msgType = WebSocketMessageType.Text;
        }

        // §9.3: Increase bufferedAmount immediately.
        BufferedAmount += (ulong)bytes.Length;

        _ = SendInternalAsync(bytes, msgType);
    }

    /// <summary>
    /// https://websockets.spec.whatwg.org/#dom-websocket-close
    /// </summary>
    public void Close(ushort? code = null, string? reason = null)
    {
        // If code is present, but is neither an integer equal to 1000 nor an integer in the range 3000 to 4999, inclusive, throw an "InvalidAccessError" DOMException.
        if (code.HasValue && code.Value != 1000 && (code.Value < 3000 || code.Value > 4999))
        {
            DOMExceptionHelper
                .CreateInvalidAccessError(
                    Engine,
                    $"Failed to execute 'close' on 'WebSocket': The code must be either 1000, or between 3000 and 4999. {code.Value} is neither."
                )
                .Throw();
        }

        // If reason is present, then run these substeps:
        //  Let reasonBytes be the result of encoding reason.
        //  If reasonBytes is longer than 123 bytes, then throw a "SyntaxError" DOMException.
        if (reason is not null && SystemEncoding.UTF8.GetByteCount(reason) > 123)
        {
            DOMExceptionHelper
                .CreateSyntaxError(
                    Engine,
                    "Failed to execute 'close' on 'WebSocket': The message must not be greater than 123 bytes."
                )
                .Throw();
        }

        // Run the first matching steps from the following list:

        // If this’s ready state is CLOSING (2) or CLOSED (3)
        //  Do nothing.
        if (ReadyState == WebSocketReadyState.Closing || ReadyState == WebSocketReadyState.Closed)
        {
            return;
        }

        switch (_client.State)
        {
            // If the WebSocket connection is not yet established [WSP]
            //  Fail the WebSocket connection and set this’s ready state to CLOSING
            case WebSocketState.Connecting:
                _client.Abort();
                ReadyState = WebSocketReadyState.Closing;
                break;

            // If the WebSocket closing handshake has not yet been started
            //  Start the WebSocket closing handshake and set this’s ready state to CLOSING
            case WebSocketState.Open:
                var closeStatus = code.HasValue
                    ? (WebSocketCloseStatus)code.Value
                    : WebSocketCloseStatus.NormalClosure;

                _ = CloseInternalAsync(closeStatus, reason ?? string.Empty);
                break;

            // Otherwise
            //  Set this’s ready state to CLOSING
            default:
                ReadyState = WebSocketReadyState.Closing;
                break;
        }
    }
}
