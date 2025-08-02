using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Streams.ReadableStream;

namespace Yilduz.Streams.ReadableStreamDefaultReader;

public sealed partial class ReadableStreamDefaultReaderInstance
{
    internal readonly List<ReadRequest> ReadRequests = [];

    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-readable-stream-default-reader
    /// </summary>
    private void SetUpDefaultReader(ReadableStreamInstance stream)
    {
        // If ! IsReadableStreamLocked(stream) is true, throw a TypeError exception.
        if (stream.Locked)
        {
            throw new JavaScriptException(
                Engine.Intrinsics.TypeError.Construct("Stream is already locked")
            );
        }

        // Perform ! ReadableStreamReaderGenericInitialize(reader, stream).
        GenericInitialize(stream);

        // Set reader.[[readRequests]] to a new empty list.
        ReadRequests.Clear();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#abstract-opdef-readablestreamdefaultreaderrelease
    /// </summary>
    internal void Release()
    {
        // Perform ! ReadableStreamReaderGenericRelease(reader).
        GenericRelease();
        // Let e be a new TypeError exception.
        // Perform ! ReadableStreamDefaultReaderErrorReadRequests(reader, e).
        ErrorReadRequests(Engine.Intrinsics.TypeError.Construct("Reader was released"));
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#abstract-opdef-readablestreamdefaultreadererrorreadrequests
    /// </summary>
    private void ErrorReadRequests(JsValue e)
    {
        // Let readRequests be reader.[[readRequests]].
        // Set reader.[[readRequests]] to a new empty list.
        // For each readRequest of readRequests,
        // Perform readRequest’s error steps, given e.

        foreach (var readRequest in ReadRequests)
        {
            readRequest.ErrorSteps(e);
        }
        ReadRequests.Clear();
    }
}
