using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Streams.WritableStreamDefaultController;

internal sealed class WritableStreamDefaultControllerPrototype : ObjectInstance
{
    private static readonly string ErrorName = nameof(Error).ToJsStyleName();
    private static readonly string SignalName = nameof(
            WritableStreamDefaultControllerInstance.Signal
        )
        .ToJsStyleName();

    public WritableStreamDefaultControllerPrototype(
        Engine engine,
        WritableStreamDefaultControllerConstructor constructor
    )
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(WritableStreamDefaultController));
        SetOwnProperty("constructor", new(constructor, false, false, false));

        FastSetProperty(
            ErrorName,
            new(new ClrFunction(Engine, ErrorName, Error), false, false, true)
        );
        FastSetProperty(
            SignalName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, SignalName.ToJsGetterName(), GetSignal),
                set: null,
                false,
                true
            )
        );
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStreamDefaultController/error
    /// </summary>
    private static JsValue Error(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WritableStreamDefaultControllerInstance>();
        var error = arguments.At(0);

        instance.Error(error);

        return Undefined;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStreamDefaultController/signal
    /// </summary>
    private static JsValue GetSignal(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<WritableStreamDefaultControllerInstance>().Signal;
    }
}
