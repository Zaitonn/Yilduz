using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Aborting.AbortSignal;

namespace Yilduz.Aborting.AbortController;

internal sealed class AbortControllerConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    public AbortControllerConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(AbortController))
    {
        PrototypeObject = new AbortControllerPrototype(engine, this);

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
        _webApiIntrinsics = webApiIntrinsics;
    }

    public AbortControllerPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new AbortControllerInstance(Engine)
        {
            Prototype = PrototypeObject,
            Signal = _webApiIntrinsics.AbortSignal.ConstructAbortSignal(),
        };
    }
}
