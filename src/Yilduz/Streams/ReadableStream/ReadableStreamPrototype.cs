using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Streams.ReadableStreamDefaultReader;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStream;

internal sealed class ReadableStreamPrototype : ObjectInstance
{
    private static readonly string LockedName = nameof(ReadableStreamInstance.Locked)
        .ToJsStyleName();
    private static readonly string LockedGetterName = LockedName.ToJsGetterName();

    private static readonly string CancelName = nameof(Cancel).ToJsStyleName();
    private static readonly string GetReaderName = nameof(GetReader).ToJsStyleName();
    private static readonly string PipeToName = nameof(PipeTo).ToJsStyleName();
    private static readonly string PipeThroughName = nameof(PipeThrough).ToJsStyleName();
    private static readonly string TeeName = nameof(Tee).ToJsStyleName();

    public ReadableStreamPrototype(Engine engine, ReadableStreamConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(ReadableStream));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        // Locked property (getter only)
        FastSetProperty(
            LockedName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, LockedGetterName, GetLocked),
                set: null,
                false,
                true
            )
        );

        // Methods
        FastSetProperty(
            CancelName,
            new(new ClrFunction(Engine, CancelName, Cancel), false, false, true)
        );
        FastSetProperty(
            GetReaderName,
            new(new ClrFunction(Engine, GetReaderName, GetReader), false, false, true)
        );
        FastSetProperty(
            PipeToName,
            new(new ClrFunction(Engine, PipeToName, PipeTo), false, false, true)
        );
        FastSetProperty(
            PipeThroughName,
            new(new ClrFunction(Engine, PipeThroughName, PipeThrough), false, false, true)
        );
        FastSetProperty(TeeName, new(new ClrFunction(Engine, TeeName, Tee), false, false, true));
    }

    private JsValue GetLocked(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ReadableStreamInstance>().Locked;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/cancel
    /// </summary>
    private JsValue Cancel(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamInstance>();
        var reason = arguments.At(0);

        return instance.Cancel(reason);
    }

    private ReadableStreamDefaultReaderInstance GetReader(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamInstance>();
        var options = arguments.At(0);

        return instance.GetReader(options);
    }

    private JsValue PipeTo(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamInstance>();
        var destination = arguments.At(0).AsObject();
        var options = arguments.At(1);

        return instance.PipeTo(destination, options);
    }

    private JsValue PipeThrough(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamInstance>();
        var transform = arguments.At(0).AsObject();
        var options = arguments.At(1).AsObject();

        // PipeThrough implementation: pipe to transform.writable and return transform.readable
        var writable = transform.Get("writable");
        var readable = transform.Get("readable");

        if (
            writable.IsNull()
            || writable.IsUndefined()
            || readable.IsNull()
            || readable.IsUndefined()
        )
        {
            TypeErrorHelper.Throw(Engine, "Invalid transform stream");
        }

        // Start the pipe operation
        instance.PipeTo(writable.AsObject(), options);

        return readable;
    }

    private JsValue Tee(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamInstance>();
        var streams = instance.Tee();
        return Engine.Intrinsics.Array.Construct([streams.Item1, streams.Item2]);
    }
}
