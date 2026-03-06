using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Utils;

namespace Yilduz.Network.XMLHttpRequestEventTarget;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequestEventTarget
/// </summary>
public sealed class XMLHttpRequestEventTargetConstructor : Constructor
{
    internal XMLHttpRequestEventTargetConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(XMLHttpRequestEventTarget))
    {
        PrototypeObject = new(_engine, this)
        {
            Prototype = webApiIntrinsics.EventTarget.PrototypeObject,
        };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.ThrowForIllegalConstructor(Engine, nameof(XMLHttpRequestEventTarget));

        return null;
    }

    internal XMLHttpRequestEventTargetPrototype PrototypeObject { get; }
}
