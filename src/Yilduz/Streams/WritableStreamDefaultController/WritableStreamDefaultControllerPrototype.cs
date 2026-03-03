using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;

namespace Yilduz.Streams.WritableStreamDefaultController;

internal sealed class WritableStreamDefaultControllerPrototype
    : PrototypeBase<WritableStreamDefaultControllerInstance>
{
    public WritableStreamDefaultControllerPrototype(
        Engine engine,
        WritableStreamDefaultControllerConstructor constructor
    )
        : base(engine, nameof(WritableStreamDefaultController), constructor)
    {
        RegisterProperty("signal", instance => instance.Signal);
        RegisterMethod("error", Error);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStreamDefaultController/error
    /// </summary>
    private static JsValue Error(
        WritableStreamDefaultControllerInstance instance,
        JsValue[] arguments
    )
    {
        var error = arguments.At(0);
        instance.Error(error);
        return Undefined;
    }
}
