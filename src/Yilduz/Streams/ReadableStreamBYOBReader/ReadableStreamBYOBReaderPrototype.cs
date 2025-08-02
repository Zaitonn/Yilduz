using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamBYOBReader;

internal sealed class ReadableStreamBYOBReaderPrototype : ObjectInstance
{
    private static readonly string ClosedName = nameof(ReadableStreamBYOBReaderInstance.Closed)
        .ToJsStyleName();
    private static readonly string ClosedGetterName = ClosedName.ToJsGetterName();

    private static readonly string CancelName = nameof(Cancel).ToJsStyleName();
    private static readonly string ReadName = nameof(Read).ToJsStyleName();
    private static readonly string ReleaseLockName = nameof(ReleaseLock).ToJsStyleName();

    public ReadableStreamBYOBReaderPrototype(
        Engine engine,
        ReadableStreamBYOBReaderConstructor constructor
    )
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(ReadableStreamBYOBReader));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        // Closed property (getter only)
        FastSetProperty(
            ClosedName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ClosedGetterName, GetClosed),
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
        FastSetProperty(ReadName, new(new ClrFunction(Engine, ReadName, Read), false, false, true));
        FastSetProperty(
            ReleaseLockName,
            new(new ClrFunction(Engine, ReleaseLockName, ReleaseLock), false, false, true)
        );
    }

    private JsValue GetClosed(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ReadableStreamBYOBReaderInstance>().Closed;
    }

    private JsValue Cancel(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamBYOBReaderInstance>();
        var reason = arguments.At(0);

        var (promise, resolve, reject) = Engine.Advanced.RegisterPromise();

        try
        {
            var result = instance.Cancel(reason);
            if (result.IsPromise())
            {
                return result;
            }
            resolve(result);
        }
        catch (JavaScriptException e)
        {
            reject(e.Error);
        }

        return promise;
    }

    private JsValue Read(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamBYOBReaderInstance>();
        var view = arguments.At(0);

        // Check if view is an ArrayBufferView (for now we'll just check if it's an object)
        if (!view.IsObject())
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    Engine.Intrinsics.TypeError.Construct(
                        "view argument must be an ArrayBufferView"
                    )
                )
                .Promise;
        }

        var (resultPromise, resolve, reject) = Engine.Advanced.RegisterPromise();

        try
        {
            var result = instance.Read(view, arguments.At(1));
            if (result.IsPromise())
            {
                return result;
            }
            resolve(result);
        }
        catch (JavaScriptException e)
        {
            reject(e.Error);
        }

        return resultPromise;
    }

    private JsValue ReleaseLock(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamBYOBReaderInstance>();
        instance.ReleaseLock();
        return Undefined;
    }
}
