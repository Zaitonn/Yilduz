using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;

namespace Yilduz.Streams.ReadableStreamDefaultController;

internal sealed class ReadableStreamDefaultControllerPrototype
    : PrototypeBase<ReadableStreamDefaultControllerInstance>
{
    public ReadableStreamDefaultControllerPrototype(
        Engine engine,
        ReadableStreamDefaultControllerConstructor constructor
    )
        : base(engine, nameof(ReadableStreamDefaultController), constructor)
    {
        RegisterProperty("desiredSize", controller => controller.DesiredSize ?? Null);

        RegisterMethod("close", Close);
        RegisterMethod("enqueue", Enqueue);
        RegisterMethod("error", Error);
    }

    private static JsValue Close(
        ReadableStreamDefaultControllerInstance controller,
        JsValue[] arguments
    )
    {
        controller.Close();
        return Undefined;
    }

    private static JsValue Enqueue(
        ReadableStreamDefaultControllerInstance controller,
        JsValue[] arguments
    )
    {
        var chunk = arguments.At(0);
        controller.Enqueue(chunk);
        return Undefined;
    }

    private static JsValue Error(
        ReadableStreamDefaultControllerInstance controller,
        JsValue[] arguments
    )
    {
        var error = arguments.At(0);
        controller.Error(error);
        return Undefined;
    }
}
