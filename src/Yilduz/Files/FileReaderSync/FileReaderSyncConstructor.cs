using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Files.FileReaderSync;

internal sealed class FileReaderSyncConstructor : Constructor
{
    public FileReaderSyncConstructor(Engine engine)
        : base(engine, nameof(FileReaderSync))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public FileReaderSyncPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new FileReaderSyncInstance(Engine) { Prototype = PrototypeObject };
    }
}
