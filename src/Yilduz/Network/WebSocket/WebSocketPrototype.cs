using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;

namespace Yilduz.Network.WebSocket;

internal sealed class WebSocketPrototype : PrototypeBase<WebSocketInstance>
{
    public WebSocketPrototype(Engine engine, WebSocketConstructor constructor)
        : base(engine, nameof(WebSocket), constructor)
    {
        // Ready state constants (also exposed on prototype per Web IDL spec)
        RegisterConstant(
            nameof(WebSocketReadyState.Connecting).ToUpperInvariant(),
            WebSocketReadyState.Connecting
        );
        RegisterConstant(
            nameof(WebSocketReadyState.Open).ToUpperInvariant(),
            WebSocketReadyState.Open
        );
        RegisterConstant(
            nameof(WebSocketReadyState.Closing).ToUpperInvariant(),
            WebSocketReadyState.Closing
        );
        RegisterConstant(
            nameof(WebSocketReadyState.Closed).ToUpperInvariant(),
            WebSocketReadyState.Closed
        );

        // Readonly getters
        RegisterProperty("url", ws => ws.Url);
        RegisterProperty("readyState", ws => (int)ws.ReadyState);
        RegisterProperty("bufferedAmount", ws => (long)ws.BufferedAmount);
        RegisterProperty("extensions", ws => ws.Extensions);
        RegisterProperty("protocol", ws => ws.Protocol);

        // Read-write properties
        RegisterProperty("binaryType", GetBinaryType, SetBinaryType);
        RegisterProperty("onopen", GetOnOpen, SetOnOpen);
        RegisterProperty("onmessage", GetOnMessage, SetOnMessage);
        RegisterProperty("onerror", GetOnError, SetOnError);
        RegisterProperty("onclose", GetOnClose, SetOnClose);

        // Methods
        RegisterMethod("send", Send);
        RegisterMethod("close", Close);
    }

    private static JsValue GetBinaryType(WebSocketInstance instance)
    {
        return instance.BinaryType;
    }

    private static JsValue SetBinaryType(WebSocketInstance instance, JsValue argument)
    {
        instance.BinaryType = argument.ToString();
        return instance.BinaryType;
    }

    private static JsValue GetOnOpen(WebSocketInstance instance)
    {
        return instance.OnOpen;
    }

    private static JsValue SetOnOpen(WebSocketInstance instance, JsValue argument)
    {
        instance.OnOpen = argument;
        return instance.OnOpen;
    }

    private static JsValue GetOnMessage(WebSocketInstance instance)
    {
        return instance.OnMessage;
    }

    private static JsValue SetOnMessage(WebSocketInstance instance, JsValue argument)
    {
        instance.OnMessage = argument;
        return instance.OnMessage;
    }

    private static JsValue GetOnError(WebSocketInstance instance)
    {
        return instance.OnError;
    }

    private static JsValue SetOnError(WebSocketInstance instance, JsValue argument)
    {
        instance.OnError = argument;
        return instance.OnError;
    }

    private static JsValue GetOnClose(WebSocketInstance instance)
    {
        return instance.OnClose;
    }

    private static JsValue SetOnClose(WebSocketInstance instance, JsValue argument)
    {
        instance.OnClose = argument;
        return instance.OnClose;
    }

    private static JsValue Send(WebSocketInstance instance, JsValue[] arguments)
    {
        instance.Send(arguments.At(0));
        return Undefined;
    }

    private static JsValue Close(WebSocketInstance instance, JsValue[] arguments)
    {
        ushort? code = null;
        string? reason = null;

        var codeArg = arguments.At(0);
        if (!codeArg.IsUndefined())
        {
            code = (ushort)codeArg.AsNumber();
        }

        var reasonArg = arguments.At(1);
        if (!reasonArg.IsUndefined())
        {
            reason = reasonArg.AsString();
        }

        instance.Close(code, reason);
        return Undefined;
    }
}
