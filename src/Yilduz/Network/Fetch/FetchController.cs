using System;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Yilduz.DOM.DOMException;
using Yilduz.Utils;

namespace Yilduz.Network.Fetch;

/// <summary>
/// https://fetch.spec.whatwg.org/#fetch-controller
/// </summary>
internal struct FetchController(Engine engine)
{
    private readonly Engine _engine = engine;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-controller-state
    /// </summary>
    public FetchControllerState State { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-controller-full-timing-info
    /// </summary>
    public FetchTimingInfo? FullTimingInfo { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-controller-report-timing-steps
    /// </summary>
    public Function? ReportTimingSteps { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-controller-next-manual-redirect-steps
    /// </summary>
    public Function? NextManualStep { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-controller-serialized-abort-reason
    /// </summary>
    public JsValue? SerializedAbortReason { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#finalize-and-report-timing
    /// </summary>
    public readonly void ReportTiming(JsValue global)
    {
        // Assert: controller’s report timing steps is non-null.
        if (ReportTimingSteps is null)
        {
            throw new InvalidOperationException("ReportTimingSteps is null");
        }

        // Call controller’s report timing steps with global.
        ReportTimingSteps.Call(global);
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-controller-process-the-next-manual-redirect
    /// </summary>
    public readonly void ProcessNextManualStep(JsValue global)
    {
        // Assert: controller’s next manual redirect steps is non-null.
        if (NextManualStep is null)
        {
            throw new InvalidOperationException("NextManualStep is null");
        }

        // Call controller’s next manual redirect steps.
        NextManualStep.Call(global);
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#extract-full-timing-info
    /// </summary>
    public readonly FetchTimingInfo? ExtractFullTimingInfo()
    {
        return FullTimingInfo;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-controller-abort
    /// </summary>
    public void Abort(JsValue error)
    {
        // Set controller’s state to "aborted".
        State = FetchControllerState.Aborted;

        // Let fallbackError be an "AbortError" DOMException.
        var fallbackError = DOMExceptionHelper.CreateAbortError(_engine);

        // Set error to fallbackError if it is not given.

        // Let serializedError be StructuredSerialize(error). If that threw an exception, catch it, and let serializedError be StructuredSerialize(fallbackError).

        // Set controller’s serialized abort reason to serializedError.
        SerializedAbortReason = error.IsUndefined() ? fallbackError : error;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-controller-terminate
    /// </summary>
    public void Terminate()
    {
        State = FetchControllerState.Terminated;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#deserialize-a-serialized-abort-reason
    /// </summary>
    public readonly DOMExceptionInstance DeserializeAbortReason(JsValue abortReason)
    {
        // Let fallbackError be an "AbortError" DOMException.
        var fallbackError = DOMExceptionHelper.CreateAbortError(_engine);

        // Let deserializedError be fallbackError.
        var deserializedError = fallbackError;

        // If abortReason is non-null, then set deserializedError to StructuredDeserialize(abortReason, realm).
        // If that threw an exception or returned undefined, then set deserializedError to fallbackError.

        // Return deserializedError.
        return deserializedError;
    }
}
