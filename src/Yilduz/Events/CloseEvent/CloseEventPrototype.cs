using Jint;
using Yilduz.Events.Event;
using Yilduz.Models;

namespace Yilduz.Events.CloseEvent;

internal sealed class CloseEventPrototype : PrototypeBase<CloseEventInstance>
{
    public CloseEventPrototype(Engine engine, CloseEventConstructor constructor)
        : base(engine, nameof(CloseEvent), constructor)
    {
        RegisterProperty("wasClean", ev => ev.WasClean);
        RegisterProperty("code", ev => (int)ev.Code);
        RegisterProperty("reason", ev => ev.Reason);

        RegisterConstant(nameof(EventPhase.AT_TARGET), EventPhase.AT_TARGET);
        RegisterConstant(nameof(EventPhase.BUBBLING_PHASE), EventPhase.BUBBLING_PHASE);
        RegisterConstant(nameof(EventPhase.CAPTURING_PHASE), EventPhase.CAPTURING_PHASE);
        RegisterConstant(nameof(EventPhase.NONE), EventPhase.NONE);
    }
}
