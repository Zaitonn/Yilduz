using System;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Promise;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Streams.ReadableStream;
using Yilduz.Streams.ReadableStreamDefaultController;
using Yilduz.Streams.TransformStreamDefaultController;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.TransformStream;

/// <summary>
/// Internal methods for TransformStreamInstance
/// </summary>
public sealed partial class TransformStreamInstance
{
    /// <summary>
    /// InitializeTransformStream
    /// <br/>
    /// https://streams.spec.whatwg.org/#initialize-transform-stream
    /// </summary>
    private void InitializeTransformStream(
        ManualPromise startPromise,
        double writableHighWaterMark,
        Function writableSizeAlgorithm,
        double readableHighWaterMark,
        Function readableSizeAlgorithm,
        out ReadableStreamInstance readable,
        out WritableStreamInstance writable
    )
    {
        var webApiIntrinsics = Engine.GetWebApiIntrinsics();
        var underlyingSink = new JsObject(Engine);

        // Let startAlgorithm be an algorithm that returns startPromise.
        underlyingSink.Set(
            "start",
            new ClrFunction(Engine, "start", (thisObj, args) => startPromise.Promise)
        );

        // Let writeAlgorithm be the following steps, taking a chunk argument:
        //   Return ! TransformStreamDefaultSinkWriteAlgorithm(stream, chunk).
        underlyingSink.Set(
            "write",
            new ClrFunction(
                Engine,
                "write",
                (thisObj, args) => DefaultSinkWriteAlgorithm(args.At(0))
            )
        );

        // Let closeAlgorithm be the following steps:
        //   Return ! TransformStreamDefaultSinkCloseAlgorithm(stream).
        underlyingSink.Set(
            "close",
            new ClrFunction(Engine, "close", (thisObj, args) => DefaultSinkCloseAlgorithm())
        );

        // Let abortAlgorithm be the following steps, taking a reason argument:
        //   Return ! TransformStreamDefaultSinkAbortAlgorithm(stream, reason).
        underlyingSink.Set(
            "abort",
            new ClrFunction(
                Engine,
                "abort",
                (thisObj, args) => DefaultSinkAbortAlgorithm(args.At(0))
            )
        );

        var writableStrategy = Engine.Intrinsics.Object.Construct(Arguments.Empty);
        writableStrategy.Set("highWaterMark", writableHighWaterMark);
        writableStrategy.Set("size", writableSizeAlgorithm);

        // Set stream.[[writable]] to ! CreateWritableStream(startAlgorithm, writeAlgorithm, closeAlgorithm, abortAlgorithm, writableHighWaterMark, writableSizeAlgorithm).
        writable = webApiIntrinsics.WritableStream.Construct(underlyingSink, writableStrategy);

        var underlyingSource = new JsObject(Engine);

        // Let pullAlgorithm be the following steps:
        //   Return ! TransformStreamDefaultSourcePullAlgorithm(stream).
        underlyingSource.Set(
            "pull",
            new ClrFunction(Engine, "pull", (thisObj, args) => DefaultSourcePullAlgorithm())
        );

        // Let cancelAlgorithm be the following steps, taking a reason argument:
        //   Return ! TransformStreamDefaultSourceCancelAlgorithm(stream, reason).
        underlyingSource.Set(
            "cancel",
            new ClrFunction(
                Engine,
                "cancel",
                (thisObj, args) => DefaultSourceCancelAlgorithm(args.At(0))
            )
        );

        var readableStrategy = new JsObject(Engine);
        readableStrategy.Set("highWaterMark", readableHighWaterMark);
        readableStrategy.Set("size", readableSizeAlgorithm);
        // Set stream.[[readable]] to ! CreateReadableStream(startAlgorithm, pullAlgorithm, cancelAlgorithm, readableHighWaterMark, readableSizeAlgorithm).
        readable = webApiIntrinsics.ReadableStream.Construct(underlyingSource, readableStrategy);

        // Set stream.[[backpressure]] and stream.[[backpressureChangePromise]] to undefined.
        Backpressure = null;
        BackpressureChangePromise = null;

        // Perform ! TransformStreamSetBackpressure(stream, true).
        SetBackpressure(true);

        // Set stream.[[controller]] to undefined.
        Controller = null;
    }

    /// <summary>
    /// SetUpTransformStreamDefaultControllerFromTransformer
    /// <br/>
    /// https://streams.spec.whatwg.org/#set-up-transform-stream-default-controller-from-transformer
    /// </summary>
    private void SetUpDefaultControllerFromTransformer(
        JsValue transformer,
        JsValue? transformerDict
    )
    {
        // Let controller be a new TransformStreamDefaultController.
        var controller = Engine
            .GetWebApiIntrinsics()
            .TransformStreamDefaultController.Construct(this);

        ClrFunction transformAlgorithm;

        // Let result be TransformStreamDefaultControllerEnqueue(controller, chunk).
        // If transformerDict["transform"] exists, set transformAlgorithm to an algorithm which takes an argument chunk and returns the result of invoking transformerDict["transform"] with argument list « chunk, controller » and callback this value transformer.
        if (
            transformerDict?.Get("transform") is { } transformMethod
            && !transformMethod.IsUndefined()
        )
        {
            transformAlgorithm = new(
                Engine,
                "transformAlgorithm",
                (_, args) =>
                    transformMethod
                        .AsFunctionInstance()
                        .Call(thisObj: transformer, [args.At(0), controller])
            );
        }
        else
        {
            // Let transformAlgorithm be the following steps, taking a chunk argument:
            transformAlgorithm = new(
                Engine,
                "transformAlgorithm",
                (_, args) =>
                {
                    var chunk = args.At(0);

                    // Let result be TransformStreamDefaultControllerEnqueue(controller, chunk).
                    try
                    {
                        controller.Enqueue(chunk);
                    }
                    catch (JavaScriptException e)
                    {
                        // If result is an abrupt completion, return a promise rejected with result.[[Value]].
                        return PromiseHelper.CreateRejectedPromise(Engine, e.Error).Promise;
                    }
                    // Otherwise, return a promise resolved with undefined.
                    return PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
                }
            );
        }

        // Let flushAlgorithm be an algorithm which returns a promise resolved with undefined.
        // If transformerDict["flush"] exists, set flushAlgorithm to an algorithm which returns the result of invoking transformerDict["flush"] with argument list « controller » and callback this value transformer.
        ClrFunction flushAlgorithm;
        if (transformerDict?.Get("flush") is { } flushMethod && !flushMethod.IsUndefined())
        {
            flushAlgorithm = new(
                Engine,
                "flushAlgorithm",
                (_, _) => flushMethod.AsFunctionInstance().Call(thisObj: transformer, [controller])
            );
        }
        else
        {
            flushAlgorithm = new(
                Engine,
                "flushAlgorithm",
                (_, _) => PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise
            );
        }

        // Let cancelAlgorithm be an algorithm which returns a promise resolved with undefined.
        ClrFunction cancelAlgorithm;
        if (transformerDict?.Get("cancel") is { } cancelMethod && !cancelMethod.IsUndefined())
        {
            cancelAlgorithm = new(
                Engine,
                "cancelAlgorithm",
                (_, args) =>
                    cancelMethod.AsFunctionInstance().Call(thisObj: transformer, [args.At(0)])
            );
        }
        else
        {
            cancelAlgorithm = new(
                Engine,
                "cancelAlgorithm",
                (_, _) => PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise
            );
        }

        // Perform ! SetUpTransformStreamDefaultController(stream, controller, transformAlgorithm, flushAlgorithm, cancelAlgorithm).
        SetUpDefaultController(controller, transformAlgorithm, flushAlgorithm, cancelAlgorithm);
    }

    /// <summary>
    /// SetUpTransformStreamDefaultController
    /// <br/>
    /// https://streams.spec.whatwg.org/#set-up-transform-stream-default-controller
    /// </summary>
    private void SetUpDefaultController(
        TransformStreamDefaultControllerInstance controller,
        Function transformAlgorithm,
        Function flushAlgorithm,
        Function cancelAlgorithm
    )
    {
        // Assert: stream implements TransformStream.
        // Assert: stream.[[controller]] is undefined.
        if (Controller is not null)
        {
            throw new InvalidOperationException("Stream controller should be undefined");
        }

        // Set controller.[[stream]] to stream.
        controller.Stream = this;

        // Set stream.[[controller]] to controller.
        Controller = controller;

        // Set controller.[[transformAlgorithm]] to transformAlgorithm.
        controller.TransformAlgorithm = transformAlgorithm;
        // Set controller.[[flushAlgorithm]] to flushAlgorithm.
        controller.FlushAlgorithm = flushAlgorithm;
        // Set controller.[[cancelAlgorithm]] to cancelAlgorithm.
        controller.CancelAlgorithm = cancelAlgorithm;
    }

    /// <summary>
    /// TransformStreamSetBackpressure
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-set-backpressure
    /// </summary>
    internal void SetBackpressure(bool backpressure)
    {
        // Assert: stream.[[backpressure]] is not backpressure.
        if (Backpressure == backpressure)
        {
            throw new InvalidOperationException("Backpressure state should change");
        }

        // If stream.[[backpressureChangePromise]] is not undefined, resolve stream.[[backpressureChangePromise]] with undefined.
        BackpressureChangePromise?.Resolve(Undefined);

        // Set stream.[[backpressureChangePromise]] to a new promise.
        BackpressureChangePromise = Engine.Advanced.RegisterPromise();

        // Set stream.[[backpressure]] to backpressure.
        Backpressure = backpressure;
    }

    #region Sink algorithms

    /// <summary>
    /// TransformStreamDefaultSinkWriteAlgorithm
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-default-sink-write-algorithm
    /// </summary>
    private JsValue DefaultSinkWriteAlgorithm(JsValue chunk)
    {
        // Assert: stream.[[writable]].[[state]] is "writable".
        if (Writable.State != WritableStreamState.Writable)
        {
            throw new InvalidOperationException("Stream is not writable");
        }

        // Let controller be stream.[[controller]].
        var controller = Controller ?? throw new InvalidOperationException("No controller");

        // If stream.[[backpressure]] is true,
        if (Backpressure == true)
        {
            // Let backpressureChangePromise be stream.[[backpressureChangePromise]].
            var backpressureChangePromise = BackpressureChangePromise;
            if (backpressureChangePromise == null)
            {
                throw new InvalidOperationException("No backpressure change promise");
            }

            // Return the result of reacting to backpressureChangePromise with the following fulfillment steps:
            return backpressureChangePromise.Promise.Then(
                onFulfilled: (_) =>
                {
                    // Let writable be stream.[[writable]].
                    var writable = Writable;

                    // Let state be writable.[[state]].
                    var state = writable.State;

                    // If state is "erroring", throw writable.[[storedError]].
                    if (state == WritableStreamState.Erroring)
                    {
                        throw new JavaScriptException(writable.StoredError);
                    }

                    // Assert: state is "writable".
                    if (state != WritableStreamState.Writable)
                    {
                        throw new InvalidOperationException("Stream state should be writable");
                    }

                    // Return ! TransformStreamDefaultControllerPerformTransform(controller, chunk).
                    return controller.PerformTransform(chunk);
                }
            );
        }

        // Return ! TransformStreamDefaultControllerPerformTransform(controller, chunk).
        return controller.PerformTransform(chunk);
    }

    /// <summary>
    /// TransformStreamDefaultSinkAbortAlgorithm
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-default-sink-abort-algorithm
    /// </summary>
    private JsValue DefaultSinkAbortAlgorithm(JsValue reason)
    {
        // Let controller be stream.[[controller]].
        if (Controller is null)
        {
            throw new InvalidOperationException("No controller");
        }

        // If controller.[[finishPromise]] is not undefined, return controller.[[finishPromise]].
        if (Controller.FinishPromise is not null)
        {
            return Controller.FinishPromise.Promise;
        }

        // Let readable be stream.[[readable]].
        // Let controller.[[finishPromise]] be a new promise.
        Controller.FinishPromise = Engine.Advanced.RegisterPromise();

        // Let cancelPromise be the result of performing controller.[[cancelAlgorithm]], passing reason.
        var cancelPromise = Controller.CancelAlgorithm?.Call(Undefined, [reason]);

        // Perform ! TransformStreamDefaultControllerClearAlgorithms(controller).
        Controller.ClearAlgorithms();

        // React to cancelPromise:
        cancelPromise?.Then(
            // If cancelPromise was fulfilled, then:
            onFulfilled: (_) =>
            {
                // If readable.[[state]] is "errored", reject controller.[[finishPromise]] with readable.[[storedError]].
                if (Readable.State == ReadableStreamState.Errored)
                {
                    Controller.FinishPromise.Reject(Readable.StoredError);
                    return Undefined;
                }

                // Otherwise:
                //   Perform ! ReadableStreamDefaultControllerError(readable.[[controller]], reason).
                ((ReadableStreamDefaultControllerInstance)Readable.Controller).ErrorInternal(
                    reason
                );

                //   Resolve controller.[[finishPromise]] with undefined.
                Controller.FinishPromise.Resolve(Undefined);
                return Undefined;
            },
            // If cancelPromise was rejected with reason r, then:
            onRejected: (r) =>
            {
                //   Perform ! ReadableStreamDefaultControllerError(readable.[[controller]], r).
                ((ReadableStreamDefaultControllerInstance)Readable.Controller).ErrorInternal(r);

                //   Reject controller.[[finishPromise]] with r.
                Controller.FinishPromise.Reject(r);

                return r;
            }
        );

        // Return controller.[[finishPromise]].
        return Controller.FinishPromise.Promise;
    }

    /// <summary>
    /// TransformStreamDefaultSinkCloseAlgorithm
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-default-sink-close-algorithm
    /// </summary>
    private JsValue DefaultSinkCloseAlgorithm()
    {
        // Let controller be stream.[[controller]].
        var controller = Controller ?? throw new InvalidOperationException("No controller");

        // If controller.[[finishPromise]] is not undefined, return controller.[[finishPromise]].
        if (controller.FinishPromise is not null)
        {
            return controller.FinishPromise.Promise;
        }

        // Let readable be stream.[[readable]].
        // Let controller.[[finishPromise]] be a new promise.
        controller.FinishPromise = Engine.Advanced.RegisterPromise();

        // Let flushPromise be the result of performing controller.[[flushAlgorithm]].
        var flushPromise = controller.FlushAlgorithm?.Call(Undefined, Arguments.Empty);

        // Perform ! TransformStreamDefaultControllerClearAlgorithms(controller).
        controller.ClearAlgorithms();

        // React to flushPromise:
        flushPromise?.Then(
            // If flushPromise was fulfilled, then:
            onFulfilled: (_) =>
            {
                // If readable.[[state]] is "errored", reject controller.[[finishPromise]] with readable.[[storedError]].
                if (Readable.State == ReadableStreamState.Errored)
                {
                    controller.FinishPromise.Reject(Readable.StoredError);
                    return Undefined;
                }

                // Otherwise:
                //   Perform ! ReadableStreamDefaultControllerClose(readable.[[controller]]).
                ((ReadableStreamDefaultControllerInstance)Readable.Controller).Close();

                //   Resolve controller.[[finishPromise]] with undefined.
                controller.FinishPromise.Resolve(Undefined);

                return Undefined;
            },
            // If flushPromise was rejected with reason r, then:
            onRejected: (r) =>
            {
                // Perform ! ReadableStreamDefaultControllerError(readable.[[controller]], r).
                ((ReadableStreamDefaultControllerInstance)Readable.Controller).ErrorInternal(r);

                // Reject controller.[[finishPromise]] with r.
                controller.FinishPromise.Reject(r);
                return r;
            }
        );

        // Return controller.[[finishPromise]].
        return controller.FinishPromise.Promise;
    }

    #endregion

    #region Source algorithms

    /// <summary>
    /// TransformStreamDefaultSourcePullAlgorithm
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-default-source-pull-algorithm
    /// </summary>
    private JsValue DefaultSourcePullAlgorithm()
    {
        // Assert: stream.[[backpressure]] is true.
        if (Backpressure != true)
        {
            throw new InvalidOperationException("Backpressure should be true");
        }

        // Assert: stream.[[backpressureChangePromise]] is not undefined.
        if (BackpressureChangePromise == null)
        {
            throw new InvalidOperationException("Backpressure change promise should be defined");
        }

        // Perform ! TransformStreamSetBackpressure(stream, false).
        SetBackpressure(false);

        // Return stream.[[backpressureChangePromise]].
        return BackpressureChangePromise.Promise;
    }

    /// <summary>
    /// TransformStreamDefaultSourceCancelAlgorithm
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-default-source-cancel-algorithm
    /// </summary>
    private JsValue DefaultSourceCancelAlgorithm(JsValue reason)
    {
        // Let controller be stream.[[controller]].
        if (Controller is null)
        {
            throw new InvalidOperationException("No controller");
        }

        // If controller.[[finishPromise]] is not undefined, return controller.[[finishPromise]].
        if (Controller.FinishPromise is not null)
        {
            return Controller.FinishPromise.Promise;
        }

        // Let writable be stream.[[writable]].
        // Let controller.[[finishPromise]] be a new promise.
        Controller.FinishPromise = Engine.Advanced.RegisterPromise();

        // Let cancelPromise be the result of performing controller.[[cancelAlgorithm]], passing reason.
        var cancelPromise = Controller.CancelAlgorithm?.Call(Undefined, [reason]);

        // Perform ! TransformStreamDefaultControllerClearAlgorithms(controller).
        Controller.ClearAlgorithms();

        // React to cancelPromise:
        cancelPromise?.Then(
            // If cancelPromise was fulfilled, then:
            onFulfilled: (_) =>
            {
                // If writable.[[state]] is "errored", reject controller.[[finishPromise]] with writable.[[storedError]].
                if (Writable.State == WritableStreamState.Errored)
                {
                    Controller.FinishPromise.Reject(Writable.StoredError);
                    return Undefined;
                }

                // Otherwise:
                //   Perform ! WritableStreamDefaultControllerErrorIfNeeded(writable.[[controller]], reason).
                Writable.Controller.ErrorIfNeeded(reason);

                //   Perform ! TransformStreamUnblockWrite(stream).
                UnblockWrite();

                //   Resolve controller.[[finishPromise]] with undefined.
                Controller.FinishPromise.Resolve(Undefined);
                return Undefined;
            },
            // If cancelPromise was rejected with reason r, then:
            onRejected: (r) =>
            {
                // Perform ! WritableStreamDefaultControllerErrorIfNeeded(writable.[[controller]], r).
                Writable.Controller?.ErrorIfNeeded(r);

                // Perform ! TransformStreamUnblockWrite(stream).
                UnblockWrite();

                // Reject controller.[[finishPromise]] with r.
                Controller.FinishPromise.Reject(r);
                return r;
            }
        );

        // For simplified implementation, return a resolved promise
        return PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
    }

    #endregion

    /// <summary>
    /// TransformStreamError
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-error
    /// </summary>
    internal void ErrorInternal(JsValue error)
    {
        // Perform ! ReadableStreamDefaultControllerError(stream.[[readable]].[[controller]], e).
        var readableController = (ReadableStreamDefaultControllerInstance)Readable.Controller;
        readableController.Error(error);

        // Perform ! TransformStreamErrorWritableAndUnblockWrite(stream, e).
        ErrorWritableAndUnblockWrite(error);
    }

    /// <summary>
    /// TransformStreamErrorWritableAndUnblockWrite
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-error-writable-and-unblock-write
    /// </summary>
    internal void ErrorWritableAndUnblockWrite(JsValue error)
    {
        // Perform ! TransformStreamDefaultControllerClearAlgorithms(stream.[[controller]]).
        Controller?.ClearAlgorithms();

        // Perform ! WritableStreamDefaultControllerErrorIfNeeded(stream.[[writable]].[[controller]], e).
        var writableController = Writable.Controller;
        writableController?.ErrorIfNeeded(error);

        // Perform ! TransformStreamUnblockWrite(stream).
        UnblockWrite();
    }

    /// <summary>
    /// TransformStreamUnblockWrite
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-unblock-write
    /// </summary>
    private void UnblockWrite()
    {
        // If stream.[[backpressure]] is true, perform ! TransformStreamSetBackpressure(stream, false).
        if (Backpressure == true)
        {
            SetBackpressure(false);
        }
    }
}
