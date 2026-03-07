using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Events.Event;
using Yilduz.Extensions;

namespace Yilduz.Events.ProgressEvent;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/ProgressEvent/ProgressEvent
/// </summary>
public sealed class ProgressEventConstructor : EventConstructor
{
    internal ProgressEventConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(ProgressEvent))
    {
        PrototypeObject = new(engine, this) { Prototype = webApiIntrinsics.Event.PrototypeObject };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private new ProgressEventPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'Event'");

        return new ProgressEventInstance(Engine, arguments.At(0), arguments.At(1))
        {
            Prototype = PrototypeObject,
        };
    }

    internal ProgressEventInstance CreateInstance(
        string type,
        ulong loaded,
        ulong total,
        bool lengthComputable
    )
    {
        return new ProgressEventInstance(Engine, Undefined, Undefined)
        {
            Prototype = PrototypeObject,
            Loaded = loaded,
            Type = type,
            Total = total,
            LengthComputable = lengthComputable,
        };
    }
}
