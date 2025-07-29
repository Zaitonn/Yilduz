using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Streams.WritableStreamDefaultWriter;

internal sealed class WritableStreamDefaultWriterPrototype : ObjectInstance
{
    private static readonly string ClosedName = nameof(WritableStreamDefaultWriterInstance.Closed)
        .ToJsStyleName();
    private static readonly string ClosedGetterName = ClosedName.ToJsGetterName();

    private static readonly string ReadyName = nameof(WritableStreamDefaultWriterInstance.Ready)
        .ToJsStyleName();
    private static readonly string ReadyGetterName = ReadyName.ToJsGetterName();

    private static readonly string DesiredSizeName = nameof(
            WritableStreamDefaultWriterInstance.DesiredSize
        )
        .ToJsStyleName();
    private static readonly string DesiredSizeGetterName = DesiredSizeName.ToJsGetterName();

    private static readonly string AbortName = nameof(Abort).ToJsStyleName();
    private static readonly string CloseName = nameof(Close).ToJsStyleName();
    private static readonly string ReleaseLockName = nameof(ReleaseLock).ToJsStyleName();
    private static readonly string WriteName = nameof(Write).ToJsStyleName();

    public WritableStreamDefaultWriterPrototype(
        Engine engine,
        WritableStreamDefaultWriterConstructor constructor
    )
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, "WritableStreamDefaultWriter");
        SetOwnProperty("constructor", new(constructor, false, false, false));

        // Properties (getters only)
        FastSetProperty(
            ClosedName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ClosedGetterName, GetClosed),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            ReadyName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ReadyGetterName, GetReady),
                set: null,
                false,
                true
            )
        );

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
            AbortName,
            new(new ClrFunction(Engine, AbortName, Abort), false, false, true)
        );
        FastSetProperty(
            CloseName,
            new(new ClrFunction(Engine, CloseName, Close), false, false, true)
        );
        FastSetProperty(
            ReleaseLockName,
            new(new ClrFunction(Engine, ReleaseLockName, ReleaseLock), false, false, true)
        );
        FastSetProperty(
            WriteName,
            new(new ClrFunction(Engine, WriteName, Write), false, false, true)
        );
    }

    private JsValue GetClosed(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<WritableStreamDefaultWriterInstance>().Closed;
    }

    private JsValue GetReady(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<WritableStreamDefaultWriterInstance>().Ready;
    }

    private JsValue GetDesiredSize(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WritableStreamDefaultWriterInstance>();
        var size = instance.DesiredSize;
        return size.HasValue ? size.Value : Null;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStreamDefaultWriter/abort
    /// </summary>
    private JsValue Abort(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WritableStreamDefaultWriterInstance>();
        var reason = arguments.At(0);

        return instance.Abort(reason);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStreamDefaultWriter/close
    /// </summary>
    private JsValue Close(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WritableStreamDefaultWriterInstance>();

        return instance.Close();
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStreamDefaultWriter/releaseLock
    /// </summary>
    private JsValue ReleaseLock(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WritableStreamDefaultWriterInstance>();

        instance.ReleaseLock();

        return Undefined;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStreamDefaultWriter/write
    /// </summary>
    private JsValue Write(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WritableStreamDefaultWriterInstance>();
        var chunk = arguments.At(0);

        return instance.Write(chunk);
    }
}
