using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Streams;

public abstract class ReadableStreamGenericReaderInstance : ObjectInstance
{
    private protected ReadableStreamGenericReaderInstance(Engine engine)
        : base(engine) { }

    public abstract JsValue Closed { get; }

    public abstract JsValue Cancel(JsValue reason);
}
