using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.DOM.DOMException;
using Yilduz.Utils;

namespace Yilduz.Network.Fetch;

internal class FetchController(Engine engine, WebApiIntrinsics webApiIntrinsics)
{
    public FetchControllerState State { get; set; }

    public FetchTimingInfo TimingInfo { get; set; }

    public JsValue ReportTimingSteps { get; set; } = JsValue.Null;

    public JsValue AbortReason { get; set; } = JsValue.Null;

    public JsValue NextManualStep { get; set; } = JsValue.Null;

    public void ReportTiming(JsValue global)
    {
        if (ReportTimingSteps.IsNull())
        {
            throw new InvalidOperationException("ReportTimingSteps is null");
        }

        if (ReportTimingSteps is ObjectInstance objectInstance)
        {
            objectInstance.Call(global);
        }
    }

    public void ProcessNextManualStep(JsValue global)
    {
        if (NextManualStep.IsNull())
        {
            throw new InvalidOperationException("ReportTimingSteps is null");
        }

        if (NextManualStep is ObjectInstance objectInstance)
        {
            objectInstance.Call(global);
        }
    }

    public void Abort(JsValue? reason)
    {
        State = FetchControllerState.Aborted;
        AbortReason = reason ?? DOMExceptionHelper.CreateAbortError(engine, "Fetch aborted");
    }

    public FetchTimingInfo ExtractFullTimingInfo()
    {
        return TimingInfo;
    }

    public void Terminate()
    {
        State = FetchControllerState.Terminated;
    }

    public DOMExceptionInstance DeserializeAbortReason(JsValue abortReason)
    {
        // Let fallbackError be an "AbortError" DOMException.
        var fallbackError = webApiIntrinsics.DOMException.CreateInstance("AbortError");

        // Let deserializedError be fallbackError.
        var deserializedError = fallbackError;

        // If abortReason is non-null, then set deserializedError to StructuredDeserialize(abortReason, realm). If that threw an exception or returned undefined, then set deserializedError to fallbackError.
        if (!abortReason.IsNull()) { }

        // Return deserializedError.
        return deserializedError;
    }
}
