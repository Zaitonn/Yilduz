using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Utils;

namespace Yilduz.Events.ProgressEvent;

internal sealed class ProgressEventConstructor : Constructor
{
    public ProgressEventConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(ProgressEvent))
    {
        PrototypeObject = new ProgressEventPrototype(engine, this)
        {
            Prototype = webApiIntrinsics.Event.PrototypeObject,
        };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public ProgressEventPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'Event'");

        return new ProgressEventInstance(Engine, arguments.At(0), arguments.At(1))
        {
            Prototype = PrototypeObject,
        };
    }
}
