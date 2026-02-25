using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.DOM.DOMException;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/DOMException
/// </summary>
public sealed class DOMExceptionConstructor : Constructor
{
    private DOMExceptionPrototype PrototypeObject { get; }

    internal DOMExceptionConstructor(Engine engine)
        : base(engine, "DOMException")
    {
        PrototypeObject = new DOMExceptionPrototype(engine, this)
        {
            Prototype = engine.Intrinsics.Error.Prototype,
        };

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));

        foreach (var pair in ErrorCodes.CodeConstantNames)
        {
            SetOwnProperty(pair.Value, new(pair.Key, false, false, false));
            PrototypeObject.FastSetProperty(pair.Value, new(pair.Key, false, false, true));
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        var message = arguments.Length > 0 ? arguments[0].ToString() : "";
        var name = arguments.Length > 1 ? arguments[1].ToString() : "Error";

        return new DOMExceptionInstance(Engine, message, name) { Prototype = PrototypeObject };
    }

    public DOMExceptionInstance CreateInstance(string name, string message = "")
    {
        return new(Engine, message, name) { Prototype = PrototypeObject };
    }
}
