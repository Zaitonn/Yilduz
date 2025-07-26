using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Utils;

namespace Yilduz.Storages.Storage;

internal sealed class StorageConstructor : Constructor
{
    public StorageConstructor(Engine engine)
        : base(engine, nameof(Storage))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public StoragePrototype PrototypeObject { get; }

    public StorageInstance CreateInstance()
    {
        return new StorageInstance(Engine) { Prototype = PrototypeObject };
    }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.Throw(Engine, "Failed to construct 'Storage': Illegal constructor");
        return null!;
    }
}
