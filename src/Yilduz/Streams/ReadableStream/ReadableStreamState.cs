namespace Yilduz.Streams.ReadableStream;

/// <summary>
/// https://streams.spec.whatwg.org/#rs-internal-slots
/// </summary>
internal enum ReadableStreamState
{
    /// <summary>
    /// The stream is readable - data can be read from it
    /// </summary>
    Readable,

    /// <summary>
    /// The stream is closed - no more data will be available
    /// </summary>
    Closed,

    /// <summary>
    /// The stream is errored - an error occurred and no more data is available
    /// </summary>
    Errored,
}
