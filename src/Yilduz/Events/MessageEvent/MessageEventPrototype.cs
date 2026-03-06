using Jint;
using Yilduz.Events.Event;
using Yilduz.Models;

namespace Yilduz.Events.MessageEvent;

internal sealed class MessageEventPrototype : PrototypeBase<MessageEventInstance>
{
    public MessageEventPrototype(Engine engine, MessageEventConstructor constructor)
        : base(engine, nameof(MessageEvent), constructor)
    {
        RegisterProperty("data", e => e.Data);
        RegisterProperty("origin", e => e.Origin);
        RegisterProperty("lastEventId", e => e.LastEventId);
        RegisterProperty("source", e => e.Source);

        RegisterConstant(nameof(EventPhase.AT_TARGET), EventPhase.AT_TARGET);
        RegisterConstant(nameof(EventPhase.BUBBLING_PHASE), EventPhase.BUBBLING_PHASE);
        RegisterConstant(nameof(EventPhase.CAPTURING_PHASE), EventPhase.CAPTURING_PHASE);
        RegisterConstant(nameof(EventPhase.NONE), EventPhase.NONE);
    }
}
