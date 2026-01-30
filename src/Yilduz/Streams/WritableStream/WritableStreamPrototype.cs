using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStream;

internal sealed class WritableStreamPrototype : ObjectInstance
{
    private static readonly string LockedName = nameof(WritableStreamInstance.Locked)
        .ToJsStyleName();
    private static readonly string LockedGetterName = LockedName.ToJsGetterName();

    private static readonly string AbortName = nameof(Abort).ToJsStyleName();
    private static readonly string CloseName = nameof(Close).ToJsStyleName();
    private static readonly string GetWriterName = nameof(GetWriter).ToJsStyleName();

    public WritableStreamPrototype(Engine engine, WritableStreamConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(WritableStream));
        SetOwnProperty("constructor", new(constructor, false, false, false));

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
            AbortName,
            new(new ClrFunction(Engine, AbortName, Abort), false, false, true)
        );
        FastSetProperty(
            CloseName,
            new(new ClrFunction(Engine, CloseName, Close), false, false, true)
        );
        FastSetProperty(
            GetWriterName,
            new(new ClrFunction(Engine, GetWriterName, GetWriter), false, false, true)
        );
    }

    private JsValue GetLocked(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<WritableStreamInstance>().Locked;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStream/abort
    /// </summary>
    private JsValue Abort(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WritableStreamInstance>();
        var reason = arguments.At(0);

        var (promise, resolve, rejected) = Engine.Advanced.RegisterPromise();

        instance
            .Abort(reason)
            .Then(
                result =>
                {
                    resolve(result);
                    return Undefined;
                },
                error =>
                {
                    rejected(error);
                    return Undefined;
                }
            );

        return promise;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStream/close
    /// </summary>
    private JsValue Close(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WritableStreamInstance>();
        instance.Close();

        return PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStream/getWriter
    /// </summary>
    private JsValue GetWriter(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<WritableStreamInstance>();
        var writer = instance.GetWriter();
        return writer;
    }
}
