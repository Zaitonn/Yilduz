using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Streams.ReadableStreamDefaultReader;

internal sealed class ReadableStreamDefaultReaderPrototype : ObjectInstance
{
    private static readonly string ClosedName = nameof(ReadableStreamDefaultReaderInstance.Closed)
        .ToJsStyleName();
    private static readonly string ClosedGetterName = ClosedName.ToJsGetterName();

    private static readonly string CancelName = nameof(Cancel).ToJsStyleName();
    private static readonly string ReadName = nameof(Read).ToJsStyleName();
    private static readonly string ReleaseLockName = nameof(ReleaseLock).ToJsStyleName();

    public ReadableStreamDefaultReaderPrototype(
        Engine engine,
        ReadableStreamDefaultReaderConstructor constructor
    )
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(ReadableStreamDefaultReader));
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

    private static JsValue GetClosed(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ReadableStreamDefaultReaderInstance>().Closed;
    }

    private JsValue Cancel(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamDefaultReaderInstance>();
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
        var instance = thisObject.EnsureThisObject<ReadableStreamDefaultReaderInstance>();

        var (promise, resolve, reject) = Engine.Advanced.RegisterPromise();

        try
        {
            var result = instance.Read();
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

    private static JsValue ReleaseLock(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamDefaultReaderInstance>();
        instance.ReleaseLock();
        return Undefined;
    }
}
