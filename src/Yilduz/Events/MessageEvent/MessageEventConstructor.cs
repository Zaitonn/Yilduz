using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Extensions;

namespace Yilduz.Events.MessageEvent;

/// <summary>
/// Constructor for <see cref="MessageEventInstance"/>.
/// https://html.spec.whatwg.org/multipage/comms.html#the-messageevent-interface
/// </summary>
internal sealed class MessageEventConstructor : Constructor
{
    public MessageEventConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(MessageEvent))
    {
        PrototypeObject = new MessageEventPrototype(engine, this)
        {
            Prototype = webApiIntrinsics.Event.PrototypeObject,
        };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public MessageEventPrototype PrototypeObject { get; }

    /// <summary>
    /// https://html.spec.whatwg.org/multipage/comms.html#dom-messageevent
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'MessageEvent'");

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

    /// <summary>
    /// Creates a <see cref="MessageEventInstance"/> with typed parameters.
    /// </summary>
    public MessageEventInstance CreateInstance(string type, JsValue data, string origin)
    {
        return new(Engine, type, data, origin) { Prototype = PrototypeObject };
    }
}
