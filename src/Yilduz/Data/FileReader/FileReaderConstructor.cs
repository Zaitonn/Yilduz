using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Data.FileReader;

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
            nameof(FileReaderReadyState.EMPTY),
            new((int)FileReaderReadyState.EMPTY, false, false, true)
        );
        SetOwnProperty(
            nameof(FileReaderReadyState.LOADING),
            new((int)FileReaderReadyState.LOADING, false, false, true)
        );
        SetOwnProperty(
            nameof(FileReaderReadyState.DONE),
            new((int)FileReaderReadyState.DONE, false, false, true)
        );
    }

    public FileReaderPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new FileReaderInstance(Engine) { Prototype = PrototypeObject };
    }
}
