using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Streams.ReadableStreamDefaultReader;

namespace Yilduz.Streams;

public abstract class ReadableStreamController : ObjectInstance
{
    private protected ReadableStreamController(Engine engine)
        : base(engine) { }

    internal abstract JsValue CancelSteps(JsValue reason);
    internal abstract void PullSteps(ReadRequest readRequest);
    internal abstract void ReleaseSteps();
}
