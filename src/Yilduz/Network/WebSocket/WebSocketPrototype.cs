using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Network.WebSocket;

internal sealed class WebSocketPrototype : ObjectInstance
{
    private static readonly string UrlName = nameof(WebSocketInstance.Url).ToJsStyleName();
    private static readonly string ReadyStateName = nameof(WebSocketInstance.ReadyState)
        .ToJsStyleName();
    private static readonly string BufferedAmountName = nameof(WebSocketInstance.BufferedAmount)
        .ToJsStyleName();
    private static readonly string ExtensionsName = nameof(WebSocketInstance.Extensions)
        .ToJsStyleName();
    private static readonly string ProtocolName = nameof(WebSocketInstance.Protocol)
        .ToJsStyleName();
    private static readonly string BinaryTypeName = nameof(WebSocketInstance.BinaryType)
        .ToJsStyleName();
    private static readonly string OnOpenName = nameof(WebSocketInstance.OnOpen).ToLowerInvariant();
    private static readonly string OnMessageName = nameof(WebSocketInstance.OnMessage)
        .ToLowerInvariant();
    private static readonly string OnErrorName = nameof(WebSocketInstance.OnError)
        .ToLowerInvariant();
    private static readonly string OnCloseName = nameof(WebSocketInstance.OnClose)
        .ToLowerInvariant();
    private static readonly string SendName = nameof(WebSocketInstance.Send).ToJsStyleName();
    private static readonly string CloseName = nameof(WebSocketInstance.Close).ToJsStyleName();

    public WebSocketPrototype(Engine engine, WebSocketConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(WebSocket));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        // Ready state constants (also exposed on prototype per Web IDL spec)
        FastSetProperty(
            nameof(WebSocketReadyState.Connecting).ToUpperInvariant(),
            new(JsNumber.Create((int)WebSocketReadyState.Connecting), false, false, false)
        );
        FastSetProperty(
            nameof(WebSocketReadyState.Open).ToUpperInvariant(),
            new(JsNumber.Create((int)WebSocketReadyState.Open), false, false, false)
        );
        FastSetProperty(
            nameof(WebSocketReadyState.Closing).ToUpperInvariant(),
            new(JsNumber.Create((int)WebSocketReadyState.Closing), false, false, false)
        );
        FastSetProperty(
            nameof(WebSocketReadyState.Closed).ToUpperInvariant(),
            new(JsNumber.Create((int)WebSocketReadyState.Closed), false, false, false)
        );

        // Readonly getters
        FastSetProperty(
            UrlName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, UrlName.ToJsGetterName(), GetUrl),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            ReadyStateName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ReadyStateName.ToJsGetterName(), GetReadyState),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            BufferedAmountName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    BufferedAmountName.ToJsGetterName(),
                    GetBufferedAmount
                ),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            ExtensionsName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ExtensionsName.ToJsGetterName(), GetExtensions),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            ProtocolName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ProtocolName.ToJsGetterName(), GetProtocol),
                set: null,
                false,
                true
            )
        );

        // Read-write properties
        FastSetProperty(
            BinaryTypeName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, BinaryTypeName.ToJsGetterName(), GetBinaryType),
                set: new ClrFunction(engine, BinaryTypeName.ToJsSetterName(), SetBinaryType),
                false,
                true
            )
        );
        FastSetProperty(
            OnOpenName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnOpenName.ToJsGetterName(), GetOnOpen),
                set: new ClrFunction(engine, OnOpenName.ToJsSetterName(), SetOnOpen),
                false,
                true
            )
        );
        FastSetProperty(
            OnMessageName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnMessageName.ToJsGetterName(), GetOnMessage),
                set: new ClrFunction(engine, OnMessageName.ToJsSetterName(), SetOnMessage),
                false,
                true
            )
        );
        FastSetProperty(
            OnErrorName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnErrorName.ToJsGetterName(), GetOnError),
                set: new ClrFunction(engine, OnErrorName.ToJsSetterName(), SetOnError),
                false,
                true
            )
        );
        FastSetProperty(
            OnCloseName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnCloseName.ToJsGetterName(), GetOnClose),
                set: new ClrFunction(engine, OnCloseName.ToJsSetterName(), SetOnClose),
                false,
                true
            )
        );

        // Methods
        FastSetProperty(SendName, new(new ClrFunction(engine, SendName, Send), false, false, true));
        FastSetProperty(
            CloseName,
            new(new ClrFunction(engine, CloseName, Close), false, false, true)
        );
    }

    private static JsValue GetUrl(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<WebSocketInstance>().Url;
    }

    private static JsValue GetReadyState(JsValue thisObject, JsValue[] arguments)
    {
        return (int)thisObject.EnsureThisObject<WebSocketInstance>().ReadyState;
    }

    private static JsValue GetBufferedAmount(JsValue thisObject, JsValue[] arguments)
    {
        return (long)thisObject.EnsureThisObject<WebSocketInstance>().BufferedAmount;
    }

    private static JsValue GetExtensions(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<WebSocketInstance>().Extensions;
    }

    private static JsValue GetProtocol(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<WebSocketInstance>().Protocol;
    }

    private static JsValue GetBinaryType(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<WebSocketInstance>().BinaryType;
    }

    private static JsValue SetBinaryType(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WebSocketInstance>();
        // Validation is handled inside the BinaryType setter
        instance.BinaryType = arguments.At(0).ToString();
        return instance.BinaryType;
    }

    private static JsValue GetOnOpen(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<WebSocketInstance>().OnOpen;
    }

    private static JsValue SetOnOpen(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WebSocketInstance>();
        instance.OnOpen = arguments.At(0);
        return instance.OnOpen;
    }

    private static JsValue GetOnMessage(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<WebSocketInstance>().OnMessage;
    }

    private static JsValue SetOnMessage(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WebSocketInstance>();
        instance.OnMessage = arguments.At(0);
        return instance.OnMessage;
    }

    private static JsValue GetOnError(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<WebSocketInstance>().OnError;
    }

    private static JsValue SetOnError(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WebSocketInstance>();
        instance.OnError = arguments.At(0);
        return instance.OnError;
    }

    private static JsValue GetOnClose(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<WebSocketInstance>().OnClose;
    }

    private static JsValue SetOnClose(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WebSocketInstance>();
        instance.OnClose = arguments.At(0);
        return instance.OnClose;
    }

    private JsValue Send(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WebSocketInstance>();
        instance.Send(arguments.At(0));
        return Undefined;
    }

    private JsValue Close(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WebSocketInstance>();
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
