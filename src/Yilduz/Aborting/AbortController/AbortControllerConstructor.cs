using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Aborting.AbortController;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/AbortController/AbortController
/// </summary>
public sealed class AbortControllerConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    internal AbortControllerConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(AbortController))
    {
        _webApiIntrinsics = webApiIntrinsics;
        PrototypeObject = new(engine, this);

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private AbortControllerPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return CreateInstance();
    }

    internal AbortControllerInstance CreateInstance()
    {
        return new(Engine)
        {
            Prototype = PrototypeObject,
            Signal = _webApiIntrinsics.AbortSignal.CreateInstance(),
        };
    }
}
