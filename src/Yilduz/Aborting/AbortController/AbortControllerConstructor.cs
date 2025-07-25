using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Aborting.AbortSignal;

namespace Yilduz.Aborting.AbortController;

internal class AbortControllerConstructor : Constructor
{
    private readonly AbortSignalConstructor _abortSignalConstructor;

    public AbortControllerConstructor(Engine engine, AbortSignalConstructor abortSignalConstructor)
        : base(engine, nameof(AbortController))
    {
        PrototypeObject = new AbortControllerPrototype(engine, this);

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
        _abortSignalConstructor = abortSignalConstructor;
    }

    public AbortControllerPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new AbortControllerInstance(Engine, _abortSignalConstructor.PrototypeObject)
        {
            Prototype = PrototypeObject,
        };
    }
}
