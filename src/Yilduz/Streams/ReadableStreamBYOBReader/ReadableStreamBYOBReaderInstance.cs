using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamBYOBReader;

/// <summary>
/// https://streams.spec.whatwg.org/#byob-reader-class
/// <br/>
/// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamBYOBReader
/// </summary>
public sealed partial class ReadableStreamBYOBReaderInstance : ReadableStreamReader
{
    /// <summary>
    /// https://streams.spec.whatwg.org/#byob-reader-constructor
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamBYOBReader/ReadableStreamBYOBReader
    /// </summary>
    internal ReadableStreamBYOBReaderInstance(Engine engine, ReadableStreamInstance stream)
        : base(engine)
    {
        SetUpBYOBReader(stream);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#byob-reader-closed
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamBYOBReader/closed
    /// </summary>
    public override JsValue Closed => ClosedPromise?.Promise ?? Null;

    /// <summary>
    /// https://streams.spec.whatwg.org/#byob-reader-cancel
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamBYOBReader/cancel
    /// </summary>
    public override JsValue Cancel(JsValue reason)
    {
        if (Stream == null)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    Engine.Intrinsics.TypeError.Construct("Reader is not attached to a stream")
                )
                .Promise;
        }

        return Stream.CancelInternal(reason);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#byob-reader-read
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamBYOBReader/read
    /// </summary>
    public JsValue Read(JsValue view, JsValue options)
    {
        // If view.[[ByteLength]] is 0, return a promise rejected with a TypeError exception.
        if (!view.IsArrayBuffer() && !view.IsDataView() && view is not JsTypedArray)
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

        var byteLength = 0d;
        if (view is JsTypedArray typedArray)
        {
            // For typed arrays, we can directly access the byte length
            byteLength = typedArray.Get("byteLength").AsNumber();
        }
        else if (view.IsArrayBuffer())
        {
            byteLength = view.AsArrayBuffer()!.Length;
        }

        // If view.[[ViewedArrayBuffer]].[[ByteLength]] is 0, return a promise rejected with a TypeError exception.
        if (byteLength == 0)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    Engine.Intrinsics.TypeError.Construct("view argument must not be empty")
                )
                .Promise;
        }

        double? min = options.IsObject() ? options.Get("min").AsNumber() : null;

        // If ! IsDetachedBuffer(view.[[ViewedArrayBuffer]]) is true, return a promise rejected with a TypeError exception.
        // TODO

        // If options["min"] is 0, return a promise rejected with a TypeError exception.
        if (min == 0)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    Engine.Intrinsics.TypeError.Construct("min option must not be 0")
                )
                .Promise;
        }

        // If view has a [[TypedArrayName]] internal slot,
        //   If options["min"] > view.[[ArrayLength]], return a promise rejected with a RangeError exception.
        // Otherwise (i.e., it is a DataView),
        //   If options["min"] > view.[[ByteLength]], return a promise rejected with a RangeError exception.
        if (min > byteLength)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(
                        Engine,
                        "RangeError",
                        $"min option ({min}) cannot be greater than the byte length of the view ({byteLength})"
                    )
                )
                .Promise;
        }

        // If this.[[stream]] is undefined, return a promise rejected with a TypeError exception.
        if (Stream == null)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    Engine.Intrinsics.TypeError.Construct("Reader is not attached to a stream")
                )
                .Promise;
        }

        // Let promise be a new promise.
        var promise = Engine.Advanced.RegisterPromise();

        // Let readIntoRequest be a new read-into request with the following items:
        var readIntoRequest = new ReadRequest(
            ChunkSteps: (chunk) =>
            {
                var result = Engine.Intrinsics.Object.Construct(Arguments.Empty);
                result.Set("value", chunk);
                result.Set("done", JsBoolean.False);
                promise.Resolve(result);
            },
            CloseSteps: (chunk) =>
            {
                var result = Engine.Intrinsics.Object.Construct(Arguments.Empty);
                result.Set("value", chunk);
                result.Set("done", JsBoolean.True);
                promise.Resolve(result);
            },
            ErrorSteps: (error) => promise.Reject(error)
        );

        // Perform ! ReadableStreamBYOBReaderRead(this, view, options["min"], readIntoRequest).
        Read(view, byteLength, min ?? 0, readIntoRequest);

        return promise.Promise;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#byob-reader-release-lock
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamBYOBReader/releaseLock
    /// </summary>
    public void ReleaseLock()
    {
        Release();
    }
}
