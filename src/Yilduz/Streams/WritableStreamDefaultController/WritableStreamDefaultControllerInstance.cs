using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Aborting.AbortController;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultController;

/// <summary>
/// WritableStreamDefaultController implementation according to WHATWG Streams Standard
/// https://streams.spec.whatwg.org/#writablestreamdefaultcontroller
/// </summary>
public sealed class WritableStreamDefaultControllerInstance : ObjectInstance
{
    /// <summary>
    /// Internal slots as defined in the WHATWG Streams specification
    /// https://streams.spec.whatwg.org/#ws-default-controller-internal-slots
    /// </summary>

    /// <summary>
    /// [[abortAlgorithm]] - A promise-returning algorithm, taking one argument (the abort reason)
    /// </summary>
    internal Func<JsValue, JsValue> AbortAlgorithm { get; set; }

    /// <summary>
    /// [[closeAlgorithm]] - A promise-returning algorithm, taking no arguments
    /// </summary>
    internal Func<JsValue, JsValue> CloseAlgorithm { get; set; }

    /// <summary>
    /// [[controlledWritableStream]] - The WritableStream instance controlled by this object
    /// </summary>
    internal WritableStreamInstance? ControlledWritableStream { get; set; }

    /// <summary>
    /// [[queue]] - A list representing the stream's internal queue of chunks
    /// </summary>
    internal List<QueueEntry> Queue { get; set; } = new();

    /// <summary>
    /// [[queueTotalSize]] - The total size of all the chunks stored in [[queue]]
    /// </summary>
    internal double QueueTotalSize { get; set; } = 0;

    /// <summary>
    /// [[started]] - A boolean flag indicating whether the underlying sink's start method has finished
    /// </summary>
    internal bool Started { get; set; } = false;

    /// <summary>
    /// [[strategyHWM]] - A number supplied by the creator as part of the stream's queuing strategy
    /// </summary>
    internal double StrategyHWM { get; set; } = 1;

    /// <summary>
    /// [[strategySizeAlgorithm]] - An algorithm to calculate the size of enqueued chunks
    /// </summary>
    internal Func<JsValue, double> StrategySizeAlgorithm { get; set; } = _ => 1;

    /// <summary>
    /// [[writeAlgorithm]] - A promise-returning algorithm, taking one argument (the chunk to write)
    /// </summary>
    internal Func<JsValue, JsValue, JsValue> WriteAlgorithm { get; set; }

    /// <summary>
    /// [[abortController]] - An AbortController instance for signaling abort
    /// </summary>
    internal AbortControllerInstance? AbortController { get; set; }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-controller-signal
    /// </summary>
    public JsValue Signal => AbortController?.Signal ?? Null;

    internal WritableStreamInstance? Stream { get; set; }

    internal WritableStreamDefaultControllerInstance(Engine engine)
        : base(engine)
    {
        AbortController = engine.GetWebApiIntrinsics().AbortController.Construct();

        // Initialize algorithms with default implementations
        AbortAlgorithm = _ => PromiseHelper.CreateResolvedPromise(engine, Undefined).Promise;
        CloseAlgorithm = _ => PromiseHelper.CreateResolvedPromise(engine, Undefined).Promise;
        WriteAlgorithm = (_, _) => PromiseHelper.CreateResolvedPromise(engine, Undefined).Promise;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-controller-error
    /// </summary>
    public void Error(JsValue error = null!)
    {
        if (ControlledWritableStream?.State != WritableStreamState.Writable)
        {
            return;
        }

        AbstractOperations.WritableStreamDefaultControllerError(this, error ?? Undefined);
    }

    /// <summary>
    /// Internal method to handle error steps
    /// </summary>
    internal void ErrorSteps()
    {
        AbstractOperations.WritableStreamDefaultControllerClearAlgorithms(this);
    }

    internal JsValue AbortSteps(JsValue reason)
    {
        var result = AbortAlgorithm(reason);
        AbstractOperations.WritableStreamDefaultControllerClearAlgorithms(this);
        return result;
    }

    /// <summary>
    /// Internal record for queue entries
    /// </summary>
    internal readonly record struct QueueEntry
    {
        public JsValue Value { get; init; }
        public double Size { get; init; }
    }
}
