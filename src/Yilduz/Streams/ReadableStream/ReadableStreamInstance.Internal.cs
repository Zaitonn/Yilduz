using System;
using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Streams.ReadableStreamDefaultController;
using Yilduz.Streams.ReadableStreamDefaultReader;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStream;

public sealed partial class ReadableStreamInstance
{
    private readonly WebApiIntrinsics _webApiIntrinsics;
    internal ReadableStreamDefaultReaderInstance? Reader { get; set; }
    internal ReadableStreamDefaultControllerInstance? Controller { get; private set; }
    internal ReadableStreamState State { get; private set; } = ReadableStreamState.Readable;
    internal bool Detached { get; private set; }
    internal bool Disturbed { get; private set; }
    internal JsValue StoredError { get; private set; } = Undefined;

    [MemberNotNullWhen(true, nameof(Reader))]
    internal bool HasDefaultReader => Reader is not null;

    /// <summary>
    /// https://streams.spec.whatwg.org/#initialize-readable-stream
    /// </summary>
    private void InitializeReadableStream()
    {
        State = ReadableStreamState.Readable;
        Reader = null;
        StoredError = Undefined;
        Disturbed = false;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-close
    /// </summary>
    internal void CloseInternal()
    {
        State = ReadableStreamState.Closed;

        if (Reader is not null)
        {
            if (Reader.ReadRequests.Count > 0)
            {
                // Fulfill all pending read requests with { value: undefined, done: true }
                while (Reader.ReadRequests.Count > 0)
                {
                    FulfillReadRequest(Undefined, true);
                }
            }
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-error
    /// </summary>
    internal void ErrorInternal(JsValue error)
    {
        if (State != ReadableStreamState.Readable)
        {
            return;
        }

        State = ReadableStreamState.Errored;
        StoredError = error;

        if (Reader is not null)
        {
            // Reject all pending read requests
            while (Reader.ReadRequests.Count > 0)
            {
                var readRequest = Reader.ReadRequests[0];
                Reader.ReadRequests.RemoveAt(0);
                readRequest.ErrorSteps.Call(Undefined, [error]);
            }
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-fulfill-read-request
    /// </summary>
    internal void FulfillReadRequest(JsValue chunk, bool done)
    {
        if (Reader is null || Reader.ReadRequests.Count == 0)
        {
            return;
        }

        var readRequest = Reader.ReadRequests[0];
        Reader.ReadRequests.RemoveAt(0);

        if (done)
        {
            readRequest.CloseSteps.Call(Undefined);
        }
        else
        {
            readRequest.ChunkSteps.Call(Undefined, [chunk]);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#acquire-readable-stream-default-reader
    /// </summary>
    internal ReadableStreamDefaultReaderInstance AcquireDefaultReader()
    {
        return _webApiIntrinsics.ReadableStreamDefaultReader.Construct(this);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-cancel
    /// </summary>
    internal JsValue CancelInternal(JsValue reason)
    {
        State = ReadableStreamState.Closed;

        if (Reader is not null)
        {
            // Reject all pending read requests
            while (Reader.ReadRequests.Count > 0)
            {
                var readRequest = Reader.ReadRequests[0];
                Reader.ReadRequests.RemoveAt(0);
                readRequest.ErrorSteps.Call(Undefined, new[] { reason });
            }
        }

        var cancelPromise = PromiseHelper.CreateResolvedPromise(Engine, Undefined);

        if (Controller?.CancelAlgorithm is not null)
        {
            try
            {
                var result = Controller.CancelAlgorithm.Call(Undefined, new[] { reason });
                if (result is not null && PromiseHelper.IsPromise(result))
                {
                    cancelPromise = PromiseHelper.CreateResolvedPromise(Engine, result);
                }
            }
            catch (JavaScriptException ex)
            {
                return PromiseHelper.CreateRejectedPromise(Engine, ex.Error).Promise;
            }
        }

        return cancelPromise.Promise;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-readable-stream-default-controller-from-underlying-source
    /// </summary>
    private void SetUpControllerFromUnderlyingSource(
        JsValue underlyingSource,
        ObjectInstance? underlyingSourceDict,
        double highWaterMark,
        Function? sizeAlgorithm
    )
    {
        var controller = _webApiIntrinsics.ReadableStreamDefaultController.Construct(
            this,
            null,
            null,
            highWaterMark,
            sizeAlgorithm
        );

        Controller = controller;

        // Extract algorithms from underlying source
        Function? startAlgorithm = null;
        Function? pullAlgorithm = null;
        Function? cancelAlgorithm = null;

        if (underlyingSourceDict is not null)
        {
            var start = underlyingSourceDict.Get("start");
            if (start is Function startFunc)
            {
                startAlgorithm = startFunc;
            }

            var pull = underlyingSourceDict.Get("pull");
            if (pull is Function pullFunc)
            {
                pullAlgorithm = pullFunc;
            }

            var cancel = underlyingSourceDict.Get("cancel");
            if (cancel is Function cancelFunc)
            {
                cancelAlgorithm = cancelFunc;
            }
        }

        controller.PullAlgorithm = pullAlgorithm;
        controller.CancelAlgorithm = cancelAlgorithm;

        // Call start algorithm if provided
        if (startAlgorithm is not null)
        {
            try
            {
                var result = startAlgorithm.Call(Undefined, new JsValue[] { controller });
                controller.Started = true;

                if (PromiseHelper.IsPromise(result))
                {
                    // Handle promise-based start
                    // For now, just mark as started
                    controller.CallPullIfNeeded();
                }
                else
                {
                    controller.CallPullIfNeeded();
                }
            }
            catch (JavaScriptException ex)
            {
                controller.ErrorInternal(ex.Error);
            }
        }
        else
        {
            controller.Started = true;
            controller.CallPullIfNeeded();
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-readable-byte-stream-controller-from-underlying-source
    /// </summary>
    private void SetUpByteControllerFromUnderlyingSource(
        JsValue underlyingSource,
        ObjectInstance? underlyingSourceDict,
        double highWaterMark
    )
    {
        // TODO: Implement byte stream controller
        throw new JavaScriptException(
            ErrorHelper.Create(Engine, "TypeError", "Byte stream controllers are not yet supported")
        );
    }

    /// <summary>
    /// Add a read request to the stream's reader
    /// https://streams.spec.whatwg.org/#readable-stream-add-read-request
    /// </summary>
    internal void AddReadRequest(ReadRequest readRequest)
    {
        if (Reader is null)
        {
            throw new InvalidOperationException("Stream does not have a reader");
        }

        Reader.ReadRequests.Add(readRequest);
    }
}
