using System;
using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
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
    /// https://streams.spec.whatwg.org/#rs-locked
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/locked
    /// </summary>
    [MemberNotNullWhen(true, nameof(Reader))]
    public bool Locked => Reader is not null;

    /// <summary>
    /// Creates a new ReadableStream object from the given handlers.
    /// https://streams.spec.whatwg.org/#rs-constructor
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/ReadableStream
    /// </summary>
    internal ReadableStreamInstance(Engine engine, JsValue underlyingSource, JsValue strategy)
        : base(engine)
    {
        _webApiIntrinsics = engine.GetWebApiIntrinsics();

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
            SetUpByteControllerFromUnderlyingSource(
                underlyingSource,
                underlyingSourceDict,
                highWaterMark
            );
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
            SetUpControllerFromUnderlyingSource(
                underlyingSource,
                underlyingSourceDict,
                highWaterMark,
                sizeAlgorithm
            );
        }
    }

    /// <summary>
    /// Returns a promise that resolves when the stream is canceled.
    /// https://streams.spec.whatwg.org/#rs-cancel
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
    /// https://streams.spec.whatwg.org/#rs-get-reader
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/getReader
    /// </summary>
    public ReadableStreamGenericReaderInstance GetReader(JsValue options)
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
    /// https://streams.spec.whatwg.org/#rs-pipe-through
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/pipeThrough
    /// </summary>
    public ReadableStreamInstance PipeThrough(ObjectInstance transform, ObjectInstance options)
    {
        if (Locked)
        {
            TypeErrorHelper.Throw(Engine, "ReadableStream is locked");
        }

        var readable = transform.Get("readable");
        var writable = transform.Get("writable");

        if (readable is not ReadableStreamInstance readableStream)
        {
            TypeErrorHelper.Throw(Engine, "transform.readable is not a ReadableStream");
            return null!;
        }

        // TODO: Implement full pipe through logic
        // For now, just return the readable stream
        return readableStream;
    }

    /// <summary>
    /// Pipes the current ReadableStream to a given WritableStream.
    /// https://streams.spec.whatwg.org/#rs-pipe-to
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/pipeTo
    /// </summary>
    public JsValue PipeTo(ObjectInstance destination, JsValue options)
    {
        if (Locked)
        {
            TypeErrorHelper.Throw(Engine, "ReadableStream is locked");
        }

        throw new NotImplementedException("PipeTo is not yet implemented");
    }

    /// <summary>
    /// Tees this readable stream, returning a two-element array containing the two resulting branches.
    /// https://streams.spec.whatwg.org/#rs-tee
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/tee
    /// </summary>
    public (ReadableStreamInstance, ReadableStreamInstance) Tee()
    {
        // TODO: Implement tee logic
        var branch1 = new ReadableStreamInstance(Engine, Undefined, Undefined);
        var branch2 = new ReadableStreamInstance(Engine, Undefined, Undefined);

        return (branch1, branch2);
    }
}
