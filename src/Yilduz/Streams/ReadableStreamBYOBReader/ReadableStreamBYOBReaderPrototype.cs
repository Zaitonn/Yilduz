using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamBYOBReader;

internal sealed class ReadableStreamBYOBReaderPrototype
    : PrototypeBase<ReadableStreamBYOBReaderInstance>
{
    public ReadableStreamBYOBReaderPrototype(
        Engine engine,
        ReadableStreamBYOBReaderConstructor constructor
    )
        : base(engine, nameof(ReadableStreamBYOBReader), constructor)
    {
        RegisterProperty("closed", reader => reader.Closed);

        RegisterMethod("cancel", Cancel);
        RegisterMethod("read", Read);
        RegisterMethod("releaseLock", ReleaseLock);
    }

    private JsValue Cancel(ReadableStreamBYOBReaderInstance reader, JsValue[] arguments)
    {
        var reason = arguments.At(0);

        var (promise, resolve, reject) = Engine.Advanced.RegisterPromise();

        try
        {
            var result = reader.Cancel(reason);
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

    private JsValue Read(ReadableStreamBYOBReaderInstance reader, JsValue[] arguments)
    {
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
            var result = reader.Read(view, arguments.At(1));
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

    private static JsValue ReleaseLock(ReadableStreamBYOBReaderInstance reader, JsValue[] arguments)
    {
        reader.ReleaseLock();
        return Undefined;
    }
}
