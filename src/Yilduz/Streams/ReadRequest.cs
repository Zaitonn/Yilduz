using System;
using Jint.Native;

namespace Yilduz.Streams;

internal record ReadRequest(
    Action<JsValue> ChunkSteps,
    Action<JsValue> CloseSteps,
    Action<JsValue> ErrorSteps
);
