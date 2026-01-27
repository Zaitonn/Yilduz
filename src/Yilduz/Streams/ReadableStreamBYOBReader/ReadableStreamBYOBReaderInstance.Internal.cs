using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Streams.ReadableStream;

namespace Yilduz.Streams.ReadableStreamBYOBReader;

public sealed partial class ReadableStreamBYOBReaderInstance
{
    internal readonly List<ReadRequest> ReadIntoRequests = [];

    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-readable-stream-byob-reader
    /// </summary>
    private void SetUpBYOBReader(ReadableStreamInstance stream)
    {
        // If ! IsReadableStreamLocked(stream) is true, throw a TypeError exception.
        if (stream.Locked)
        {
            throw new JavaScriptException(
                Engine.Intrinsics.TypeError.Construct("Stream is already locked")
            );
        }

        // If stream.[[controller]] does not implement ReadableByteStreamController, throw a TypeError exception.

        // Perform ! ReadableStreamReaderGenericInitialize(reader, stream).
        GenericInitialize(stream);

        // Set reader.[[readIntoRequests]] to a new empty list.
        ReadIntoRequests.Clear();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#abstract-opdef-readablestreambyobrelease
    /// </summary>
    internal override void Release()
    {
        GenericRelease();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#abstract-opdef-readablestreambyobreadererrorreadintoRequests
    /// </summary>
    private void ErrorReadIntoRequests(JsValue e)
    {
        while (ReadIntoRequests.Count > 0)
        {
            var readRequest = ReadIntoRequests[0];
            ReadIntoRequests.RemoveAt(0);
            readRequest.ErrorSteps(e);
        }
    }

    /// <summary>
    ///  https://streams.spec.whatwg.org/#readable-stream-byob-reader-read
    /// </summary>
    private void Read(JsValue view, double byteLength, double min, ReadRequest readIntoRequest)
    {
        // Let stream be reader.[[stream]].
        // Assert: stream is not undefined.
        if (Stream is null)
        {
            throw new NotSupportedException();
        }

        // Set stream.[[disturbed]] to true.
        Stream.Disturbed = true;

        // If stream.[[state]] is "errored", perform readIntoRequest’s error steps given stream.[[storedError]].
        if (Stream.State == ReadableStreamState.Errored)
        {
            readIntoRequest.ErrorSteps(Stream.StoredError);
        }
        // Otherwise, perform ! ReadableByteStreamControllerPullInto(stream.[[controller]], view, min, readIntoRequest).
        else
        {
            PullInto(view, byteLength, min, readIntoRequest);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-byte-stream-controller-pull-into
    /// </summary>
    private void PullInto(JsValue view, double byteLength, double min, ReadRequest readIntoRequest)
    {
        // Let elementSize be 1.
        var elementSize = 1u;

        // Let ctor be %DataView%.
        Constructor ctor;

        // If view has a [[TypedArrayName]] internal slot (i.e., it is not a DataView),
        if (view is JsTypedArray typedArray)
        {
            // Set elementSize to the element size specified in the typed array constructors table for view.[[TypedArrayName]].
            elementSize = typedArray.Length;

            // Set ctor to the constructor specified in the typed array constructors table for view.[[TypedArrayName]].
            ctor =
                typedArray.IsBigInt64Array() ? Engine.Intrinsics.BigInt64Array
                : typedArray.IsBigUint64Array() ? Engine.Intrinsics.BigUint64Array
                : typedArray.IsFloat32Array() ? Engine.Intrinsics.Float32Array
                : typedArray.IsFloat64Array() ? Engine.Intrinsics.Float64Array
                : throw new NotSupportedException("Unsupported typed array type");
        }

        // Let minimumFill be min × elementSize.
        var minimumFill = min * elementSize;

        // Assert: minimumFill ≥ 0 and minimumFill ≤ view.[[ByteLength]].
        // Assert: the remainder after dividing minimumFill by elementSize is 0.
        if (minimumFill < 0 || minimumFill > byteLength || minimumFill % elementSize != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumFill));
        }

        // TODO

        // Let byteOffset be view.[[ByteOffset]].
        var byteOffset = view is JsTypedArray ta ? ta.Get("byteOffset").AsNumber() : 0;

        // Let byteLength be view.[[ByteLength]].

        // Let bufferResult be TransferArrayBuffer(view.[[ViewedArrayBuffer]]).

        // If bufferResult is an abrupt completion,
        //   Perform readIntoRequest’s error steps given bufferResult.[[value]].
        //   Return.

        // Let buffer be bufferResult.[[Value]].
        // Let pullIntoDescriptor be a new pull-into descriptor with
        //   buffer -> buffer
        //   buffer byte length -> buffer.[[ArrayBufferByteLength]]
        //   byte offset -> byteOffset
        //   byte length -> byteLength
        //   bytes filled -> 0
        //   minimum fill -> minimumFill
        //   element size -> elementSize
        //   view constructor -> ctor
        //   reader type -> "byob"
    }
}
