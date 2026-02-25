using Jint;
using Jint.Native.Function;
using Yilduz.Network.Request;
using Yilduz.Network.Response;

namespace Yilduz.Network.Fetch;

/// <summary>
/// https://fetch.spec.whatwg.org/#fetch-params
/// </summary>
internal sealed record FetchParams(Engine engine)
{
    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-request
    /// </summary>
    public RequestConcept? Request { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-process-request-body-chunk-length
    /// </summary>
    public ulong? ProcessRequestBodyChunkLength { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-process-request-end-of-body
    /// </summary>
    public Function? ProcessRequestEndOfBody { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-process-early-hints-response
    /// </summary>
    public Function? ProcessEarlyHintsResponse { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-process-response
    /// </summary>
    public Function? ProcessResponse { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-process-response-end-of-body
    /// </summary>
    public Function? ProcessResponseEndOfBody { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-process-response-consume-body
    /// </summary>
    public Function? ProcessResponseConsumeBody { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-task-destination
    /// </summary>
    public object? TaskDestination { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-cross-origin-isolated-capability
    /// </summary>
    public bool CrossOriginIsolatedCapability { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-controller
    /// </summary>
    public FetchController Controller { get; set; } = new(engine);

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-timing-info
    /// </summary>
    public FetchTimingInfo TimingInfo { get; set; } = new();

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-preloaded-response-candidate
    /// </summary>
    public object? PreloadedResponseCandidate { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-aborted
    /// </summary>
    public bool IsAborted => Controller.State == FetchControllerState.Aborted;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-params-canceled
    /// </summary>
    public bool IsCanceled =>
        Controller.State is FetchControllerState.Aborted or FetchControllerState.Terminated;
}
