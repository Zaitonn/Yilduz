using Jint.Native.Function;

namespace Yilduz.Streams.ReadableStreamDefaultReader;

internal record ReadRequest(Function ChunkSteps, Function CloseSteps, Function ErrorSteps);
