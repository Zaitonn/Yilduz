using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;

namespace Yilduz.Streams.TransformStreamDefaultController;

internal sealed class TransformStreamDefaultControllerPrototype
    : PrototypeBase<TransformStreamDefaultControllerInstance>
{
    public TransformStreamDefaultControllerPrototype(
        Engine engine,
        TransformStreamDefaultControllerConstructor constructor
    )
        : base(engine, nameof(TransformStreamDefaultController), constructor)
    {
        RegisterProperty("desiredSize", controller => controller.DesiredSize ?? Null);

        RegisterMethod("enqueue", Enqueue);
        RegisterMethod("error", Error);
        RegisterMethod("terminate", Terminate);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ts-default-controller-enqueue
    /// </summary>
    private static JsValue Enqueue(
        TransformStreamDefaultControllerInstance instance,
        JsValue[] arguments
    )
    {
        var chunk = arguments.At(0);
        instance.Enqueue(chunk);
        return Undefined;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ts-default-controller-error
    /// </summary>
    private static JsValue Error(
        TransformStreamDefaultControllerInstance instance,
        JsValue[] arguments
    )
    {
        var e = arguments.At(0);
        instance.Error(e);
        return Undefined;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ts-default-controller-terminate
    /// </summary>
    private static JsValue Terminate(
        TransformStreamDefaultControllerInstance instance,
        JsValue[] arguments
    )
    {
        instance.Terminate();
        return Undefined;
    }
}
