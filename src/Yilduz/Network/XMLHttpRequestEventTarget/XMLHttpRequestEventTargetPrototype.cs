using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Network.XMLHttpRequestEventTarget;

internal sealed class XMLHttpRequestEventTargetPrototype : ObjectInstance
{
    private static readonly string OnAbortName = nameof(XMLHttpRequestEventTargetInstance.OnAbort)
        .ToLowerInvariant();
    private static readonly string OnErrorName = nameof(XMLHttpRequestEventTargetInstance.OnError)
        .ToLowerInvariant();
    private static readonly string OnLoadName = nameof(XMLHttpRequestEventTargetInstance.OnLoad)
        .ToLowerInvariant();
    private static readonly string OnLoadStartName = nameof(
            XMLHttpRequestEventTargetInstance.OnLoadStart
        )
        .ToLowerInvariant();
    private static readonly string OnProgressName = nameof(
            XMLHttpRequestEventTargetInstance.OnProgress
        )
        .ToLowerInvariant();
    private static readonly string OnTimeoutName = nameof(
            XMLHttpRequestEventTargetInstance.OnTimeout
        )
        .ToLowerInvariant();
    private static readonly string OnLoadEndName = nameof(
            XMLHttpRequestEventTargetInstance.OnLoadEnd
        )
        .ToLowerInvariant();

    public XMLHttpRequestEventTargetPrototype(
        Engine engine,
        XMLHttpRequestEventTargetConstructor constructor
    )
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(XMLHttpRequestEventTarget));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        // Event handler properties
        FastSetProperty(
            OnAbortName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnAbortName.ToJsGetterName(), GetOnAbort),
                set: new ClrFunction(engine, OnAbortName.ToJsSetterName(), SetOnAbort),
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
            OnLoadName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnLoadName.ToJsGetterName(), GetOnLoad),
                set: new ClrFunction(engine, OnLoadName.ToJsSetterName(), SetOnLoad),
                false,
                true
            )
        );
        FastSetProperty(
            OnLoadStartName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnLoadStartName.ToJsGetterName(), GetOnLoadStart),
                set: new ClrFunction(engine, OnLoadStartName.ToJsSetterName(), SetOnLoadStart),
                false,
                true
            )
        );
        FastSetProperty(
            OnProgressName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnProgressName.ToJsGetterName(), GetOnProgress),
                set: new ClrFunction(engine, OnProgressName.ToJsSetterName(), SetOnProgress),
                false,
                true
            )
        );
        FastSetProperty(
            OnTimeoutName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnTimeoutName.ToJsGetterName(), GetOnTimeout),
                set: new ClrFunction(engine, OnTimeoutName.ToJsSetterName(), SetOnTimeout),
                false,
                true
            )
        );
        FastSetProperty(
            OnLoadEndName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnLoadEndName.ToJsGetterName(), GetOnLoadEnd),
                set: new ClrFunction(engine, OnLoadEndName.ToJsSetterName(), SetOnLoadEnd),
                false,
                true
            )
        );
    }

    private JsValue GetOnAbort(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>().OnAbort;
    }

    private JsValue SetOnAbort(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>();
        instance.OnAbort = arguments.At(0);
        return instance.OnAbort;
    }

    private JsValue GetOnError(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>().OnError;
    }

    private JsValue SetOnError(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>();
        instance.OnError = arguments.At(0);
        return instance.OnError;
    }

    private JsValue GetOnLoad(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>().OnLoad;
    }

    private JsValue SetOnLoad(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>();
        instance.OnLoad = arguments.At(0);
        return instance.OnLoad;
    }

    private JsValue GetOnLoadStart(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>().OnLoadStart;
    }

    private JsValue SetOnLoadStart(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>();
        instance.OnLoadStart = arguments.At(0);
        return instance.OnLoadStart;
    }

    private JsValue GetOnProgress(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>().OnProgress;
    }

    private JsValue SetOnProgress(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>();
        instance.OnProgress = arguments.At(0);
        return instance.OnProgress;
    }

    private JsValue GetOnTimeout(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>().OnTimeout;
    }

    private JsValue SetOnTimeout(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>();
        instance.OnTimeout = arguments.At(0);
        return instance.OnTimeout;
    }

    private JsValue GetOnLoadEnd(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>().OnLoadEnd;
    }

    private JsValue SetOnLoadEnd(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestEventTargetInstance>();
        instance.OnLoadEnd = arguments.At(0);
        return instance.OnLoadEnd;
    }
}
