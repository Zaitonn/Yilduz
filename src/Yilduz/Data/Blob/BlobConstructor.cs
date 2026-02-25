using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Utils;

namespace Yilduz.Data.Blob;

internal sealed class BlobConstructor : Constructor
{
    public BlobConstructor(Engine engine)
        : base(engine, nameof(Blob))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public BlobPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        try
        {
            return new BlobInstance(Engine, arguments.At(0), arguments.At(1))
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
}
