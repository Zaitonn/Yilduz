using System;
using Jint.Native;

namespace Yilduz.Streams.ReadableStreamDefaultReader;

internal record ReadRequest(
    Action<JsValue> ChunkSteps,
    Action CloseSteps,
    Action<JsValue> ErrorSteps
);
