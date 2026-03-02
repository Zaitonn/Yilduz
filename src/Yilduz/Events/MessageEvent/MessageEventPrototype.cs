using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Events.MessageEvent;

/// <summary>
/// Prototype for <see cref="MessageEventInstance"/>.
/// https://html.spec.whatwg.org/multipage/comms.html#the-messageevent-interface
/// </summary>
internal sealed class MessageEventPrototype : ObjectInstance
{
    public MessageEventPrototype(Engine engine, MessageEventConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(MessageEvent));
        FastSetProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(
            "data",
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, "get data", GetData),
                set: null,
                enumerable: true,
                configurable: true
            )
        );

        FastSetProperty(
            "origin",
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, "get origin", GetOrigin),
                set: null,
                enumerable: true,
                configurable: true
            )
        );

        FastSetProperty(
            "lastEventId",
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, "get lastEventId", GetLastEventId),
                set: null,
                enumerable: true,
                configurable: true
            )
        );

        FastSetProperty(
            "source",
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, "get source", GetSource),
                set: null,
                enumerable: true,
                configurable: true
            )
        );
    }

    private static JsValue GetData(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<MessageEventInstance>().Data;
    }

    private static JsValue GetOrigin(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<MessageEventInstance>().Origin;
    }

    private static JsValue GetLastEventId(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<MessageEventInstance>().LastEventId;
    }

    private static JsValue GetSource(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<MessageEventInstance>().Source;
    }
}
