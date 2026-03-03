using Jint;
using Yilduz.Models;

namespace Yilduz.Events.CloseEvent;

/// <summary>
/// Prototype for <see cref="CloseEventInstance"/>.
/// https://websockets.spec.whatwg.org/#the-closeevent-interface
/// </summary>
internal sealed class CloseEventPrototype : PrototypeBase<CloseEventInstance>
{
    public CloseEventPrototype(Engine engine, CloseEventConstructor constructor)
        : base(engine, nameof(CloseEvent), constructor)
    {
        RegisterProperty("wasClean", ev => ev.WasClean);
        RegisterProperty("code", ev => (int)ev.Code);
        RegisterProperty("reason", ev => ev.Reason);
    }
}
