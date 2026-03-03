using Jint;
using Jint.Native;
using Yilduz.Models;

namespace Yilduz.DOM.DOMException;

internal class DOMExceptionPrototype : PrototypeBase<DOMExceptionInstance>
{
    internal DOMExceptionPrototype(Engine engine, DOMExceptionConstructor constructor)
        : base(engine, nameof(DOMException), constructor)
    {
        RegisterProperty("name", GetName);
        RegisterProperty("message", GetMessage);
        RegisterProperty("code", GetCode);

        RegisterMethod("toString", ToString);
    }

    private static JsValue GetName(DOMExceptionInstance thisObject)
    {
        return thisObject.Name;
    }

    private static JsValue GetMessage(DOMExceptionInstance thisObject)
    {
        return thisObject.Message;
    }

    private static JsValue GetCode(DOMExceptionInstance thisObject)
    {
        return thisObject.Code;
    }

    private static JsValue ToString(DOMExceptionInstance thisObject, JsValue[] arguments)
    {
        return thisObject.ToString();
    }
}
