using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Utils;

namespace Yilduz.Network.Fetch;

internal class FetchController(Engine engine)
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
            return;
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
            return;
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

    public void Terminate()
    {
        State = FetchControllerState.Terminated;
    }
}
