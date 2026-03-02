using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Extensions;

namespace Yilduz.Events.CloseEvent;

/// <summary>
/// Constructor for <see cref="CloseEventInstance"/>.
/// https://websockets.spec.whatwg.org/#the-closeevent-interface
/// </summary>
internal sealed class CloseEventConstructor : Constructor
{
    public CloseEventConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(CloseEvent))
    {
        PrototypeObject = new CloseEventPrototype(engine, this)
        {
            Prototype = webApiIntrinsics.Event.PrototypeObject,
        };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public CloseEventPrototype PrototypeObject { get; }

    /// <summary>
    /// https://websockets.spec.whatwg.org/#dom-closeevent
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'CloseEvent'");

        var wasClean = false;
        ushort code = 0;
        var reason = string.Empty;

        var init = arguments.At(1);
        if (init.IsObject())
        {
            var wasCleanVal = init.Get("wasClean");
            if (!wasCleanVal.IsUndefined())
            {
                wasClean = wasCleanVal.ConvertToBoolean();
            }

            var codeVal = init.Get("code");
            if (codeVal.IsNumber())
            {
                code = (ushort)codeVal.AsNumber();
            }

            var reasonVal = init.Get("reason");
            if (!reasonVal.IsUndefined())
            {
                reason = reasonVal.ToString();
            }
        }

        return CreateInstance(wasClean, code, reason);
    }

    /// <summary>
    /// Creates a <see cref="CloseEventInstance"/> with typed parameters.
    /// https://websockets.spec.whatwg.org/#the-closeevent-interface
    /// </summary>
    public CloseEventInstance CreateInstance(bool wasClean, ushort code, string reason)
    {
        return new CloseEventInstance(Engine, wasClean, code, reason)
        {
            Prototype = PrototypeObject,
        };
    }
}
