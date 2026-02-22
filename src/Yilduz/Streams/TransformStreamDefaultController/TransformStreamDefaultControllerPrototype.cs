using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Streams.TransformStreamDefaultController;

internal sealed class TransformStreamDefaultControllerPrototype : ObjectInstance
{
    private static readonly string DesiredSizeName = nameof(
            TransformStreamDefaultControllerInstance.DesiredSize
        )
        .ToJsStyleName();
    private static readonly string DesiredSizeGetterName = DesiredSizeName.ToJsGetterName();

    private static readonly string EnqueueName = nameof(Enqueue).ToJsStyleName();
    private static readonly string ErrorName = nameof(Error).ToJsStyleName();
    private static readonly string TerminateName = nameof(Terminate).ToJsStyleName();

    private readonly TransformStreamDefaultControllerConstructor _constructor;

    public TransformStreamDefaultControllerPrototype(
        Engine engine,
        TransformStreamDefaultControllerConstructor constructor
    )
        : base(engine)
    {
        _constructor = constructor;
        Set(GlobalSymbolRegistry.ToStringTag, nameof(TransformStreamDefaultController));
        SetOwnProperty("constructor", new(_constructor, false, false, true));
        DefineProperties();
    }

    private void DefineProperties()
    {
        // DesiredSize property (getter only)
        FastSetProperty(
            DesiredSizeName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, DesiredSizeGetterName, GetDesiredSize),
                set: null,
                true,
                true
            )
        );

        // Methods
        FastSetProperty(
            EnqueueName,
            new(new ClrFunction(Engine, EnqueueName, Enqueue), false, false, true)
        );
        FastSetProperty(
            ErrorName,
            new(new ClrFunction(Engine, ErrorName, Error), false, false, true)
        );
        FastSetProperty(
            TerminateName,
            new(new ClrFunction(Engine, TerminateName, Terminate), false, false, true)
        );
    }

    private static JsValue GetDesiredSize(JsValue thisObject, JsValue[] arguments)
    {
        var controller = thisObject.EnsureThisObject<TransformStreamDefaultControllerInstance>();
        var desiredSize = controller.DesiredSize;
        return desiredSize.HasValue ? JsNumber.Create(desiredSize.Value) : Null;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ts-default-controller-enqueue
    /// </summary>
    private static JsValue Enqueue(JsValue thisObject, JsValue[] arguments)
    {
        var controller = thisObject.EnsureThisObject<TransformStreamDefaultControllerInstance>();
        var chunk = arguments.At(0);
        controller.Enqueue(chunk);
        return Undefined;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ts-default-controller-error
    /// </summary>
    private static JsValue Error(JsValue thisObject, JsValue[] arguments)
    {
        var controller = thisObject.EnsureThisObject<TransformStreamDefaultControllerInstance>();
        var e = arguments.At(0);
        controller.Error(e);
        return Undefined;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ts-default-controller-terminate
    /// </summary>
    private static JsValue Terminate(JsValue thisObject, JsValue[] arguments)
    {
        var controller = thisObject.EnsureThisObject<TransformStreamDefaultControllerInstance>();
        controller.Terminate();
        return Undefined;
    }
}
