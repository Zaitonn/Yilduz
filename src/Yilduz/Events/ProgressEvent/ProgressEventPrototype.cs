using Jint;
using Yilduz.Events.Event;
using Yilduz.Models;

namespace Yilduz.Events.ProgressEvent;

internal sealed class ProgressEventPrototype : PrototypeBase<ProgressEventInstance>
{
    public ProgressEventPrototype(Engine engine, ProgressEventConstructor constructor)
        : base(engine, nameof(ProgressEvent), constructor)
    {
        RegisterProperty("lengthComputable", e => e.LengthComputable);
        RegisterProperty("loaded", e => e.Loaded);
        RegisterProperty("total", e => e.Total);

        RegisterConstant(nameof(EventPhase.AT_TARGET), EventPhase.AT_TARGET);
        RegisterConstant(nameof(EventPhase.BUBBLING_PHASE), EventPhase.BUBBLING_PHASE);
        RegisterConstant(nameof(EventPhase.CAPTURING_PHASE), EventPhase.CAPTURING_PHASE);
        RegisterConstant(nameof(EventPhase.NONE), EventPhase.NONE);
    }
}
