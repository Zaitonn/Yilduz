using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Streams.ReadableStreamDefaultController;

internal sealed class ReadableStreamDefaultControllerPrototype : ObjectInstance
{
    private static readonly string DesiredSizeName = nameof(
            ReadableStreamDefaultControllerInstance.DesiredSize
        )
        .ToJsStyleName();
    private static readonly string DesiredSizeGetterName = DesiredSizeName.ToJsGetterName();

    private static readonly string CloseName = nameof(Close).ToJsStyleName();
    private static readonly string EnqueueName = nameof(Enqueue).ToJsStyleName();
    private static readonly string ErrorName = nameof(Error).ToJsStyleName();

    public ReadableStreamDefaultControllerPrototype(
        Engine engine,
        ReadableStreamDefaultControllerConstructor constructor
    )
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(ReadableStreamDefaultController));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        // DesiredSize property (getter only)
        FastSetProperty(
            DesiredSizeName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, DesiredSizeGetterName, GetDesiredSize),
                set: null,
                false,
                true
            )
        );

        // Methods
        FastSetProperty(
            CloseName,
            new(new ClrFunction(Engine, CloseName, Close), false, false, true)
        );
        FastSetProperty(
            EnqueueName,
            new(new ClrFunction(Engine, EnqueueName, Enqueue), false, false, true)
        );
        FastSetProperty(
            ErrorName,
            new(new ClrFunction(Engine, ErrorName, Error), false, false, true)
        );
    }

    private static JsValue GetDesiredSize(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamDefaultControllerInstance>();
        var desiredSize = instance.DesiredSize;
        return desiredSize.HasValue ? JsNumber.Create(desiredSize.Value) : Null;
    }

    private static JsValue Close(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamDefaultControllerInstance>();
        instance.Close();
        return Undefined;
    }

    private static JsValue Enqueue(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamDefaultControllerInstance>();
        var chunk = arguments.At(0);
        instance.Enqueue(chunk);
        return Undefined;
    }

    private static JsValue Error(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamDefaultControllerInstance>();
        var error = arguments.At(0);
        instance.Error(error);
        return Undefined;
    }
}
