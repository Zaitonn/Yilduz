using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.DOM.DOMException;

internal class DOMExceptionPrototype : ObjectInstance
{
    private static readonly string NameProperty = nameof(DOMExceptionInstance.Name).ToJsStyleName();
    private static readonly string MessageProperty = nameof(DOMExceptionInstance.Message)
        .ToJsStyleName();
    private static readonly string CodeProperty = nameof(DOMExceptionInstance.Code).ToJsStyleName();
    private static readonly string ToStringProperty = nameof(ToString).ToJsStyleName();

    internal DOMExceptionPrototype(Engine engine, DOMExceptionConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(DOMException));
        FastSetProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(
            NameProperty,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, NameProperty.ToJsGetterName(), GetName),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            MessageProperty,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, MessageProperty.ToJsGetterName(), GetMessage),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            CodeProperty,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, CodeProperty.ToJsGetterName(), GetCode),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            CodeProperty,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, CodeProperty.ToJsGetterName(), GetCode),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            ToStringProperty,
            new(
                new ClrFunction(engine, ToStringProperty.ToJsGetterName(), ToString),
                false,
                false,
                true
            )
        );
    }

    private JsValue GetName(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<DOMExceptionInstance>().Name;
    }

    private JsValue GetMessage(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<DOMExceptionInstance>().Message;
    }

    private JsValue GetCode(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<DOMExceptionInstance>().Code;
    }

    private JsValue ToString(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<DOMExceptionInstance>().ToString();
    }
}
