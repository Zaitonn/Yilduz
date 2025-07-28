using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Data.Files.FileReader;

internal sealed class FileReaderConstructor : Constructor
{
    public FileReaderConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(FileReader))
    {
        PrototypeObject = new(engine, this)
        {
            Prototype = webApiIntrinsics.EventTarget.PrototypeObject,
        };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));

        SetOwnProperty(
            nameof(FileReaderState.EMPTY),
            new((int)FileReaderState.EMPTY, false, false, true)
        );
        SetOwnProperty(
            nameof(FileReaderState.LOADING),
            new((int)FileReaderState.LOADING, false, false, true)
        );
        SetOwnProperty(
            nameof(FileReaderState.DONE),
            new((int)FileReaderState.DONE, false, false, true)
        );
    }

    public FileReaderPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new FileReaderInstance(Engine) { Prototype = PrototypeObject };
    }
}
