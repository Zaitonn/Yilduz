using System;
using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime.Interop;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStream;

public sealed partial class ReadableStreamInstance
{
    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-readable-byte-stream-controller-from-underlying-source
    /// </summary>
    [MemberNotNull(nameof(Controller))]
    private void SetUpByteControllerFromUnderlyingSource(
        JsValue underlyingSource,
        object? _,
        double highWaterMark
    )
    {
        // Let controller be a new ReadableByteStreamController.
        // var controller = _webApiIntrinsics.ReadableStreamDefaultController.Construct(this);

        // Let startAlgorithm be an algorithm that returns undefined.
        Function startAlgorithm = new ClrFunction(Engine, string.Empty, (_, _) => Undefined);

        var emptyPromiseFunction = new ClrFunction(
            Engine,
            string.Empty,
            (_, _) => PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise
        );
        // Let pullAlgorithm be an algorithm that returns a promise resolved with undefined.
        Function pullAlgorithm = emptyPromiseFunction;

        // Let cancelAlgorithm be an algorithm that returns a promise resolved with undefined.
        Function cancelAlgorithm = emptyPromiseFunction;

        double? autoAllocateChunkSize = null;
        if (underlyingSource.IsObject())
        {
            // If underlyingSourceDict["start"] exists, then set startAlgorithm to an algorithm which returns the result of invoking underlyingSourceDict["start"] with argument list « controller » and callback this value underlyingSource.
            var start = underlyingSource.Get("start");
            if (!start.IsUndefined())
            {
                startAlgorithm = start.AsFunctionInstance();
            }

            // If underlyingSourceDict["pull"] exists, then set pullAlgorithm to an algorithm which returns the result of invoking underlyingSourceDict["pull"] with argument list « controller » and callback this value underlyingSource.
            var pull = underlyingSource.Get("pull");
            if (!pull.IsUndefined())
            {
                pullAlgorithm = pull.AsFunctionInstance();
            }

            // If underlyingSourceDict["cancel"] exists, then set cancelAlgorithm to an algorithm which takes an argument reason and returns the result of invoking underlyingSourceDict["cancel"] with argument list « reason » and callback this value underlyingSource.
            var cancel = underlyingSource.Get("cancel");
            if (!cancel.IsUndefined())
            {
                cancelAlgorithm = cancel.AsFunctionInstance();
            }

            // Let autoAllocateChunkSize be underlyingSourceDict["autoAllocateChunkSize"], if it exists, or undefined otherwise.
            var autoAllocateChunkSizeProperty = underlyingSource.Get("autoAllocateChunkSize");
            if (!autoAllocateChunkSizeProperty.IsUndefined())
            {
                autoAllocateChunkSize = autoAllocateChunkSizeProperty.AsNumber();
            }
        }

        // If autoAllocateChunkSize is 0, then throw a TypeError exception.
        if (autoAllocateChunkSize == 0)
        {
            TypeErrorHelper.Throw(Engine, "autoAllocateChunkSize cannot be 0");
        }

        // Perform ? SetUpReadableByteStreamController(stream, controller, startAlgorithm, pullAlgorithm, cancelAlgorithm, highWaterMark, autoAllocateChunkSize).
        SetUpReadableByteStreamController(
            startAlgorithm,
            pullAlgorithm,
            cancelAlgorithm,
            highWaterMark,
            autoAllocateChunkSize
        );
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-readable-byte-stream-controller
    /// </summary>
    [MemberNotNull(nameof(Controller))]
    private void SetUpReadableByteStreamController(
        Function startAlgorithm,
        Function pullAlgorithm,
        Function cancelAlgorithm,
        double highWaterMark,
        double? autoAllocateChunkSize
    )
    {
        // Assert: stream.[[controller]] is undefined.

        // If autoAllocateChunkSize is not undefined,
        if (autoAllocateChunkSize is not null)
        {
            // Assert: ! IsInteger(autoAllocateChunkSize) is true.
            if ((long)autoAllocateChunkSize.Value != autoAllocateChunkSize.Value)
            {
                TypeErrorHelper.Throw(Engine, "autoAllocateChunkSize must be an integer");
            }

            // Assert: autoAllocateChunkSize is positive.
            if (autoAllocateChunkSize.Value <= 0)
            {
                TypeErrorHelper.Throw(Engine, "autoAllocateChunkSize must be positive");
            }
        }

        // Set controller.[[stream]] to stream.
        // Set controller.[[pullAgain]] and controller.[[pulling]] to false.
        // Set controller.[[byobRequest]] to null.

        // Perform ! ResetQueue(controller).
        // controller.ResetQueue();

        // Set controller.[[closeRequested]] and controller.[[started]] to false.
        // Set controller.[[strategyHWM]] to highWaterMark.
        // Set controller.[[pullAlgorithm]] to pullAlgorithm.
        // Set controller.[[cancelAlgorithm]] to cancelAlgorithm.
        // Set controller.[[autoAllocateChunkSize]] to autoAllocateChunkSize.
        // Set controller.[[pendingPullIntos]] to a new empty list.

        // Set stream.[[controller]] to controller.
        // Controller = _webApiIntrinsics.ReadableStreamDefaultController.Construct(
        //     this,
        //     pullAlgorithm,
        //     cancelAlgorithm,
        //     highWaterMark,
        //     autoAllocateChunkSize
        // );

        throw new NotImplementedException();

        // try
        // {
        //     // Let startResult be the result of performing startAlgorithm.
        //     startAlgorithm.Call(Controller, [Controller]);

        //     // Let startPromise be a promise resolved with startResult.

        //     // Upon fulfillment of startPromise,
        //     // Set controller.[[started]] to true.
        //     Controller.Started = true;

        //     // Assert: controller.[[pulling]] is false.
        //     // Assert: controller.[[pullAgain]] is false.

        //     // Perform ! ReadableByteStreamControllerCallPullIfNeeded(controller).
        //     Controller.CallPullIfNeeded();
        // }
        // catch (JavaScriptException e)
        // {
        //     // Upon rejection of startPromise with reason r,
        //     // Perform ! ReadableByteStreamControllerError(controller, r).
        //     Controller.ErrorInternal(e.Error);
        // }
    }
}
