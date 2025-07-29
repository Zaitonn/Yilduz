using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Promise;
using Jint.Runtime;
using Yilduz.Streams.WritableStreamDefaultController;
using Yilduz.Streams.WritableStreamDefaultWriter;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStream;

/// <summary>
/// WritableStream implementation according to WHATWG Streams Standard
/// https://streams.spec.whatwg.org/#ws-class
/// </summary>
public sealed class WritableStreamInstance : ObjectInstance
{
    /// <summary>
    /// Internal slots as defined in the WHATWG Streams specification
    /// https://streams.spec.whatwg.org/#ws-internal-slots
    /// </summary>

    /// <summary>
    /// [[backpressure]] - A boolean indicating the backpressure signal set by the controller
    /// </summary>
    public bool Backpressure { get; set; }

    /// <summary>
    /// [[closeRequest]] - The promise returned from the writer's close() method
    /// </summary>
    public ManualPromise? CloseRequest { get; set; }

    /// <summary>
    /// [[controller]] - A WritableStreamDefaultController created with the ability to control the state and queue of this stream
    /// </summary>
    public WritableStreamDefaultControllerInstance? Controller { get; set; }

    /// <summary>
    /// [[Detached]] - A boolean flag set to true when the stream is transferred
    /// </summary>
    public bool Detached { get; set; }

    /// <summary>
    /// [[inFlightWriteRequest]] - A slot set to the promise for the current in-flight write operation
    /// </summary>
    public ManualPromise? InFlightWriteRequest { get; set; }

    /// <summary>
    /// [[inFlightCloseRequest]] - A slot set to the promise for the current in-flight close operation
    /// </summary>
    public ManualPromise? InFlightCloseRequest { get; set; }

    /// <summary>
    /// [[pendingAbortRequest]] - A pending abort request
    /// </summary>
    internal PendingAbortRequest? PendingAbortRequest { get; set; }

    /// <summary>
    /// [[state]] - A string containing the stream's current state
    /// </summary>
    public WritableStreamState State { get; set; }

    /// <summary>
    /// [[storedError]] - A value indicating how the stream failed
    /// </summary>
    public JsValue StoredError { get; set; } = Undefined;

    /// <summary>
    /// [[writer]] - A WritableStreamDefaultWriter instance, if the stream is locked to a writer
    /// </summary>
    public WritableStreamDefaultWriterInstance? Writer { get; set; }

    /// <summary>
    /// [[writeRequests]] - A list of promises representing the stream's internal queue of write requests
    /// </summary>
    public List<ManualPromise> WriteRequests { get; set; } = new();

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-locked
    /// </summary>
    public bool Locked => Writer != null;

    /// <summary>
    /// Internal constructor for creating WritableStream instances
    /// </summary>
    internal WritableStreamInstance(Engine engine)
        : base(engine) { }

    /// <summary>
    /// Constructor for user-created WritableStream instances
    /// https://streams.spec.whatwg.org/#ws-constructor
    /// </summary>
    public WritableStreamInstance(Engine engine, JsValue underlyingSink, JsValue strategy)
        : base(engine)
    {
        var underlyingSinkDict = underlyingSink.IsObject() ? underlyingSink.AsObject() : null;

        // Step 3: If underlyingSinkDict["type"] exists, throw a RangeError exception
        if (underlyingSinkDict?.HasProperty("type") == true)
        {
            throw new JavaScriptException(
                ErrorHelper.Create(
                    Engine,
                    "RangeError",
                    "WritableStream constructor does not support a type property"
                )
            );
        }

        // Step 4: Perform ! InitializeWritableStream(this)
        AbstractOperations.InitializeWritableStream(this);

        // Step 5: Let sizeAlgorithm be ! ExtractSizeAlgorithm(strategy)
        var sizeAlgorithm = ExtractSizeAlgorithm(strategy);

        // Step 6: Let highWaterMark be ? ExtractHighWaterMark(strategy, 1)
        var highWaterMark = ExtractHighWaterMark(strategy, 1);

        // Step 7: Perform ? SetUpWritableStreamDefaultControllerFromUnderlyingSink
        WritableStreamDefaultController.AbstractOperations.SetUpWritableStreamDefaultControllerFromUnderlyingSink(
            this,
            underlyingSink,
            underlyingSinkDict,
            highWaterMark,
            sizeAlgorithm
        );
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-abort
    /// </summary>
    public JsValue Abort(JsValue reason = null!)
    {
        if (AbstractOperations.IsWritableStreamLocked(this))
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is locked")
                )
                .Promise;
        }

        return AbstractOperations.WritableStreamAbort(this, reason ?? Undefined);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-close
    /// </summary>
    public JsValue Close()
    {
        if (AbstractOperations.IsWritableStreamLocked(this))
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is locked")
                )
                .Promise;
        }

        if (AbstractOperations.WritableStreamCloseQueuedOrInFlight(this))
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is already being closed")
                )
                .Promise;
        }

        return AbstractOperations.WritableStreamClose(this);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-get-writer
    /// </summary>
    public WritableStreamDefaultWriterInstance GetWriter()
    {
        return AbstractOperations.AcquireWritableStreamDefaultWriter(this);
    }

    /// <summary>
    /// Extract size algorithm from strategy
    /// https://streams.spec.whatwg.org/#extract-size-algorithm
    /// </summary>
    private Func<JsValue, double> ExtractSizeAlgorithm(JsValue strategy)
    {
        if (strategy.IsObject())
        {
            var sizeProperty = strategy.Get("size");
            if (!sizeProperty.IsUndefined())
            {
                return chunk =>
                {
                    try
                    {
                        var result = Engine.Call(sizeProperty, strategy, new[] { chunk });
                        var number = TypeConverter.ToNumber(result);

                        if (double.IsNaN(number) || double.IsInfinity(number))
                        {
                            throw new JavaScriptException(
                                ErrorHelper.Create(
                                    Engine,
                                    "RangeError",
                                    "Invalid size returned by size algorithm"
                                )
                            );
                        }

                        return number;
                    }
                    catch (Exception ex) when (!(ex is JavaScriptException))
                    {
                        throw new JavaScriptException(
                            ErrorHelper.Create(
                                Engine,
                                "TypeError",
                                "Size algorithm threw an exception"
                            )
                        );
                    }
                };
            }
        }

        return _ => 1.0;
    }

    /// <summary>
    /// Extract high water mark from strategy
    /// https://streams.spec.whatwg.org/#extract-high-water-mark
    /// </summary>
    private double ExtractHighWaterMark(JsValue strategy, double defaultHWM)
    {
        if (strategy.IsObject())
        {
            var highWaterMarkProperty = strategy.Get("highWaterMark");
            if (!highWaterMarkProperty.IsUndefined())
            {
                var number = TypeConverter.ToNumber(highWaterMarkProperty);

                if (double.IsNaN(number) || number < 0)
                {
                    throw new JavaScriptException(
                        ErrorHelper.Create(Engine, "RangeError", "Invalid highWaterMark value")
                    );
                }

                return number;
            }
        }

        return defaultHWM;
    }
}
