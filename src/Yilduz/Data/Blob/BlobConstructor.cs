using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Utils;

namespace Yilduz.Data.Blob;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/Blob/Blob
/// </summary>
public sealed class BlobConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    internal BlobConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(Blob))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
        _webApiIntrinsics = webApiIntrinsics;
    }

    internal BlobPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        try
        {
            return new BlobInstance(Engine, _webApiIntrinsics, arguments.At(0), arguments.At(1))
            {
                Prototype = PrototypeObject,
            };
        }
        catch (Exception e) when (e is not JavaScriptException)
        {
            TypeErrorHelper.Throw(Engine, "Failed to construct 'Blob': " + e.Message);
            return null;
        }
    }

    internal BlobInstance CreateInstance(byte[] bytes, string mimeType)
    {
        var instance = new BlobInstance(Engine, _webApiIntrinsics, Undefined, Undefined)
        {
            Prototype = PrototypeObject,
            Type = mimeType,
        };
        instance.Value.AddRange(bytes);
        return instance;
    }
}
