using Jint;
using Jint.Native;
using Jint.Native.Object;

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
        return Construct();
    }

    public AbortControllerInstance Construct()
    {
        return new AbortControllerInstance(Engine)
        {
            Prototype = PrototypeObject,
            Signal = _webApiIntrinsics.AbortSignal.ConstructAbortSignal(),
        };
    }
}
