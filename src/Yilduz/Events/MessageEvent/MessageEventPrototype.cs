using Jint;
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
    }
}
