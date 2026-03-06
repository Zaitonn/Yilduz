using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.Data.File;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/File/File
/// </summary>
public sealed class FileConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    internal FileConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(File))
    {
        _webApiIntrinsics = webApiIntrinsics;
        PrototypeObject = new(engine, this) { Prototype = _webApiIntrinsics.Blob.PrototypeObject };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private FilePrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCountForConstructor(Engine, 2, nameof(File));

        try
        {
            return new FileInstance(
                Engine,
                _webApiIntrinsics,
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
            return null;
        }
    }
}
