using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Data.Files.Blob;
using Yilduz.Utils;

namespace Yilduz.Data.Files.File;

internal sealed class FileConstructor : Constructor
{
    private readonly BlobConstructor _blobConstructor;

    public FileConstructor(Engine engine, BlobConstructor blobConstructor)
        : base(engine, nameof(File))
    {
        PrototypeObject = new(engine, this) { Prototype = blobConstructor.PrototypeObject };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
        _blobConstructor = blobConstructor;
    }

    public FilePrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 2, "Failed to construct 'File'");

        try
        {
            return new FileInstance(
                Engine,
                _blobConstructor,
                arguments.At(0),
                arguments.At(1),
                arguments.At(2)
            )
            {
                Prototype = PrototypeObject,
            };
        }
        catch (Exception e) when (e is not JavaScriptException)
        {
            TypeErrorHelper.Throw(Engine, "Failed to construct 'File': " + e.Message);
            return null!;
        }
    }
}
