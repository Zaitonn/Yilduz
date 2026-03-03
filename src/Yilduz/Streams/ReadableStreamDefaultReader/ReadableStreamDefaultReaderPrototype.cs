using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;

namespace Yilduz.Streams.ReadableStreamDefaultReader;

internal sealed class ReadableStreamDefaultReaderPrototype
    : PrototypeBase<ReadableStreamDefaultReaderInstance>
{
    public ReadableStreamDefaultReaderPrototype(
        Engine engine,
        ReadableStreamDefaultReaderConstructor constructor
    )
        : base(engine, nameof(ReadableStreamDefaultReader), constructor)
    {
        RegisterProperty("closed", reader => reader.Closed);

        RegisterMethod("cancel", Cancel);
        RegisterMethod("read", Read);
        RegisterMethod("releaseLock", ReleaseLock);
    }

    private JsValue Cancel(ReadableStreamDefaultReaderInstance reader, JsValue[] arguments)
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

    private JsValue Read(ReadableStreamDefaultReaderInstance reader, JsValue[] arguments)
    {
        var (promise, resolve, reject) = Engine.Advanced.RegisterPromise();

        try
        {
            var result = reader.Read();
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

    private static JsValue ReleaseLock(
        ReadableStreamDefaultReaderInstance reader,
        JsValue[] arguments
    )
    {
        reader.ReleaseLock();
        return Undefined;
    }
}
