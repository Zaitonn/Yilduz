using Jint;
using Jint.Native;
using Yilduz.Events.Event;
using Yilduz.Extensions;

namespace Yilduz.Events.ProgressEvent;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/ProgressEvent
/// </summary>
public sealed class ProgressEventInstance : EventInstance
{
    internal ProgressEventInstance(Engine engine, JsValue type, JsValue options)
        : base(engine, type, options)
    {
        if (!options.IsObject())
        {
            return;
        }

        LengthComputable = options.Get("lengthComputable").ToBoolean();

        var loaded = options.Get("loaded");
        if (loaded.IsNumber())
        {
            Loaded = (ulong)loaded.AsNumber();
        }

        var total = options.Get("total");
        if (total.IsNumber())
        {
            Total = (ulong)total.AsNumber();
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ProgressEvent/lengthComputable
    /// </summary>
    public bool LengthComputable { get; internal set; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ProgressEvent/loaded
    /// </summary>
    public ulong Loaded { get; internal set; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ProgressEvent/total
    /// </summary>
    public ulong Total { get; internal set; }
}
