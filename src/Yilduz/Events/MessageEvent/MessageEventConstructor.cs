using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Events.Event;
using Yilduz.Extensions;

namespace Yilduz.Events.MessageEvent;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/MessageEvent/MessageEvent
/// <br/>
/// https://html.spec.whatwg.org/multipage/comms.html#the-messageevent-interface
/// </summary>
public sealed class MessageEventConstructor : EventConstructor
{
    internal MessageEventConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(MessageEvent))
    {
        PrototypeObject = new(engine, this) { Prototype = webApiIntrinsics.Event.PrototypeObject };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private new MessageEventPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCountForConstructor(Engine, 1, nameof(MessageEvent));

        var type = arguments.At(0).ToString();
        var init = arguments.At(1);

        JsValue data = Null;
        var origin = string.Empty;

        if (init.IsObject())
        {
            var dataVal = init.Get("data");
            if (!dataVal.IsUndefined())
            {
                data = dataVal;
            }

            var originVal = init.Get("origin");
            if (!originVal.IsUndefined())
            {
                origin = originVal.ToString();
            }
        }

        return CreateInstance(type, data, origin);
    }

    internal MessageEventInstance CreateInstance(string type, JsValue data, string origin)
    {
        return new(Engine, type, data, origin) { Prototype = PrototypeObject };
    }
}
