using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Streams.TransformStream;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStream;

/// <summary>
/// https://streams.spec.whatwg.org/#rs-class
/// <br />
/// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream
/// </summary>
public sealed partial class ReadableStreamInstance : ObjectInstance
{
    /// <summary>
    /// Returns a boolean indicating whether or not the readable stream is locked to a reader.
    /// <br/>
    /// https://streams.spec.whatwg.org/#rs-locked
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/locked
    /// </summary>
    [MemberNotNullWhen(true, nameof(Reader))]
    public bool Locked => Reader is not null;

    /// <summary>
    /// Creates a new ReadableStream object from the given handlers.
    /// <br/>
    /// https://streams.spec.whatwg.org/#rs-constructor
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/ReadableStream
    /// </summary>
    internal ReadableStreamInstance(
        WebApiIntrinsics webApiIntrinsics,
        Engine engine,
        JsValue underlyingSource,
        JsValue strategy
    )
        : base(engine)
    {
        _webApiIntrinsics = webApiIntrinsics;

        // If underlyingSource is missing, set it to null.
        underlyingSource = underlyingSource.IsUndefined() ? Null : underlyingSource;

        // Let underlyingSourceDict be underlyingSource, converted to an IDL value of type UnderlyingSource.
        var underlyingSourceDict = underlyingSource.IsObject() ? underlyingSource.AsObject() : null;

        // Perform ! InitializeReadableStream(this).
        InitializeReadableStream();

        var type = underlyingSourceDict?.Get("type");

        // If underlyingSourceDict["type"] is "bytes":
        if (type?.ToString() == "bytes")
        {
            if (strategy is ObjectInstance strategyObject)
            {
                var size = strategyObject.Get("size");

                // If strategy["size"] exists, throw a RangeError exception.
                if (!size.IsNull() && !size.IsUndefined())
                {
                    throw new JavaScriptException(
                        ErrorHelper.Create(
                            Engine,
                            "RangeError",
                            "ReadableStream constructor does not support a size property in the strategy object when the type is 'bytes'"
                        )
                    );
                }
            }

            // Let highWaterMark be ? ExtractHighWaterMark(strategy, 0).
            var highWaterMark = AbstractOperations.ExtractHighWaterMark(engine, strategy, 0);

            // Perform ? SetUpReadableByteStreamControllerFromUnderlyingSource(this, underlyingSource, underlyingSourceDict, highWaterMark).
            SetUpByteControllerFromUnderlyingSource(underlyingSourceDict, highWaterMark);
        }
        // Otherwise,
        else
        {
            // Assert: underlyingSourceDict["type"] does not exist.
            if (type is not null && !type.IsNull() && !type.IsUndefined())
            {
                throw new JavaScriptException(
                    ErrorHelper.Create(
                        Engine,
                        "RangeError",
                        "ReadableStream constructor does not support a type property"
                    )
                );
            }

            // Let sizeAlgorithm be ! ExtractSizeAlgorithm(strategy).
            // Let highWaterMark be ? ExtractHighWaterMark(strategy, 1).
            var (highWaterMark, sizeAlgorithm) = AbstractOperations.ExtractQueuingStrategy(
                engine,
                strategy,
                1
            );

            // Perform ? SetUpReadableStreamDefaultControllerFromUnderlyingSource(this, underlyingSource, underlyingSourceDict, highWaterMark, sizeAlgorithm).
            SetUpControllerFromUnderlyingSource(underlyingSourceDict, highWaterMark, sizeAlgorithm);
        }
    }

    /// <summary>
    /// Returns a promise that resolves when the stream is canceled.
    /// <br/>
    /// https://streams.spec.whatwg.org/#rs-cancel
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/cancel
    /// </summary>
    public JsValue Cancel(JsValue reason)
    {
        if (Locked)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "ReadableStream is locked")
                )
                .Promise;
        }

        return CancelInternal(reason);
    }

    /// <summary>
    /// Creates a reader and locks the stream to it.
    /// <br/>
    /// https://streams.spec.whatwg.org/#rs-get-reader
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/getReader
    /// </summary>
    public ReadableStreamReader GetReader(JsValue options)
    {
        var mode = options.IsUndefined() ? null : options.AsObject().Get("mode");

        if (mode is null || mode.IsUndefined())
        {
            // Return ? AcquireReadableStreamDefaultReader(this)
            return AcquireDefaultReader();
        }

        if (mode != "byob")
        {
            TypeErrorHelper.Throw(
                Engine,
                "ReadableStream.getReader() options.mode must be 'byob' or undefined"
            );
        }

        return AcquireBYOBReader();
    }

    /// <summary>
    /// Provides a chainable way of piping the current stream through a transform stream.
    /// <br/>
    /// https://streams.spec.whatwg.org/#rs-pipe-through
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/pipeThrough
    /// </summary>
    public ReadableStreamInstance PipeThrough(ObjectInstance transform, JsValue options)
    {
        // If ! IsReadableStreamLocked(this) is true, throw a TypeError exception.
        if (Locked)
        {
            TypeErrorHelper.Throw(Engine, "ReadableStream is locked");
        }

        WritableStreamInstance writable;
        ReadableStreamInstance readable;

        if (transform is IGenericTransformStream transformStream)
        {
            readable = transformStream.Readable;
            writable = transformStream.Writable;
        }
        else
        {
            readable =
                transform.Get("readable").As<ReadableStreamInstance>()
                ?? throw new JavaScriptException(
                    ErrorHelper.Create(
                        Engine,
                        "TypeError",
                        "transform.readable is not a ReadableStream"
                    )
                );
            writable =
                transform.Get("writable").As<WritableStreamInstance>()
                ?? throw new JavaScriptException(
                    ErrorHelper.Create(
                        Engine,
                        "TypeError",
                        "transform.writable is not a WritableStream"
                    )
                );
        }

        // If ! IsWritableStreamLocked(transform["writable"]) is true, throw a TypeError exception.
        if (writable.Locked)
        {
            TypeErrorHelper.Throw(Engine, "transform.writable is not an unlocked WritableStream");
        }

        if (readable.Locked)
        {
            TypeErrorHelper.Throw(Engine, "transform.readable is not an unlocked ReadableStream");
        }

        var signal = options.Get("signal");
        var abortSignal = signal.IsUndefined() ? null : (AbortSignalInstance)signal;

        var preventClose = options.Get("preventClose");
        var preventAbort = options.Get("preventAbort");
        var preventCancel = options.Get("preventCancel");

        // Let promise be ! ReadableStreamPipeTo(this, transform["writable"], options["preventClose"], options["preventAbort"], options["preventCancel"], signal).
        // Set promise.[[PromiseIsHandled]] to true.
        PipeToInternal(
            writable,
            !preventClose.IsUndefined() && preventClose.AsBoolean(),
            !preventAbort.IsUndefined() && preventAbort.AsBoolean(),
            !preventCancel.IsUndefined() && preventCancel.AsBoolean(),
            abortSignal
        );

        return readable;
    }

    /// <summary>
    /// Pipes the current ReadableStream to a given WritableStream.
    /// <br/>
    /// https://streams.spec.whatwg.org/#rs-pipe-to
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/pipeTo
    /// </summary>
    public JsValue PipeTo(ObjectInstance destination, JsValue options)
    {
        // If ! IsReadableStreamLocked(this) is true, return a promise rejected with a TypeError exception.
        if (Locked)
        {
            TypeErrorHelper.Throw(Engine, "ReadableStream is locked");
        }

        if (destination is not WritableStreamInstance writableStream)
        {
            TypeErrorHelper.Throw(Engine, "destination is not a WritableStream");
            throw null;
        }

        // If ! IsWritableStreamLocked(destination) is true, return a promise rejected with a TypeError exception.
        if (writableStream.Locked)
        {
            TypeErrorHelper.Throw(Engine, "WritableStream is locked");
        }

        // Let signal be options["signal"] if it exists, or undefined otherwise.
        // Return ! ReadableStreamPipeTo(this, destination, options["preventClose"], options["preventAbort"], options["preventCancel"], signal).
        if (options.IsUndefined())
        {
            return PipeToInternal(writableStream, false, false, false, null);
        }

        var optionsObject = options.AsObject();
        var preventClose = optionsObject.Get("preventClose");
        var preventAbort = optionsObject.Get("preventAbort");
        var preventCancel = optionsObject.Get("preventCancel");
        var signal = optionsObject.Get("signal");

        return PipeToInternal(
            writableStream,
            !preventClose.IsUndefined() && preventClose.AsBoolean(),
            !preventAbort.IsUndefined() && preventAbort.AsBoolean(),
            !preventCancel.IsUndefined() && preventCancel.AsBoolean(),
            signal.IsUndefined() ? null : (AbortSignalInstance)signal
        );
    }

    /// <summary>
    /// Tees this readable stream, returning a two-element array containing the two resulting branches.
    /// <br/>
    /// https://streams.spec.whatwg.org/#rs-tee
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/tee
    /// </summary>
    public (ReadableStreamInstance, ReadableStreamInstance) Tee()
    {
        // Return ? ReadableStreamTee(this, false).
        return TeeInternal(false);
    }
}
