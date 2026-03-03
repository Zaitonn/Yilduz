using Jint;
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
    }
}
