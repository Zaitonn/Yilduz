using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;

namespace Yilduz.Network.XMLHttpRequestEventTarget;

internal sealed class XMLHttpRequestEventTargetPrototype
    : PrototypeBase<XMLHttpRequestEventTargetInstance>
{
    public XMLHttpRequestEventTargetPrototype(
        Engine engine,
        XMLHttpRequestEventTargetConstructor constructor
    )
        : base(engine, nameof(XMLHttpRequestEventTarget), constructor)
    {
        RegisterProperty("onabort", GetOnAbort, SetOnAbort, Types.Object);
        RegisterProperty("onerror", GetOnError, SetOnError, Types.Object);
        RegisterProperty("onload", GetOnLoad, SetOnLoad, Types.Object);
        RegisterProperty("onloadstart", GetOnLoadStart, SetOnLoadStart, Types.Object);
        RegisterProperty("onprogress", GetOnProgress, SetOnProgress, Types.Object);
        RegisterProperty("ontimeout", GetOnTimeout, SetOnTimeout, Types.Object);
        RegisterProperty("onloadend", GetOnLoadEnd, SetOnLoadEnd, Types.Object);
    }

    private static JsValue GetOnAbort(XMLHttpRequestEventTargetInstance instance)
    {
        return instance.OnAbort;
    }

    private static JsValue SetOnAbort(XMLHttpRequestEventTargetInstance instance, JsValue argument)
    {
        instance.OnAbort = argument;
        return instance.OnAbort;
    }

    private static JsValue GetOnError(XMLHttpRequestEventTargetInstance instance)
    {
        return instance.OnError;
    }

    private static JsValue SetOnError(XMLHttpRequestEventTargetInstance instance, JsValue argument)
    {
        instance.OnError = argument;
        return instance.OnError;
    }

    private static JsValue GetOnLoad(XMLHttpRequestEventTargetInstance instance)
    {
        return instance.OnLoad;
    }

    private static JsValue SetOnLoad(XMLHttpRequestEventTargetInstance instance, JsValue argument)
    {
        instance.OnLoad = argument;
        return instance.OnLoad;
    }

    private static JsValue GetOnLoadStart(XMLHttpRequestEventTargetInstance instance)
    {
        return instance.OnLoadStart;
    }

    private static JsValue SetOnLoadStart(
        XMLHttpRequestEventTargetInstance instance,
        JsValue argument
    )
    {
        instance.OnLoadStart = argument;
        return instance.OnLoadStart;
    }

    private static JsValue GetOnProgress(XMLHttpRequestEventTargetInstance instance)
    {
        return instance.OnProgress;
    }

    private static JsValue SetOnProgress(
        XMLHttpRequestEventTargetInstance instance,
        JsValue argument
    )
    {
        instance.OnProgress = argument;
        return instance.OnProgress;
    }

    private static JsValue GetOnTimeout(XMLHttpRequestEventTargetInstance instance)
    {
        return instance.OnTimeout;
    }

    private static JsValue SetOnTimeout(
        XMLHttpRequestEventTargetInstance instance,
        JsValue argument
    )
    {
        instance.OnTimeout = argument;
        return instance.OnTimeout;
    }

    private static JsValue GetOnLoadEnd(XMLHttpRequestEventTargetInstance instance)
    {
        return instance.OnLoadEnd;
    }

    private static JsValue SetOnLoadEnd(
        XMLHttpRequestEventTargetInstance instance,
        JsValue argument
    )
    {
        instance.OnLoadEnd = argument;
        return instance.OnLoadEnd;
    }
}
