using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Data.FileReaderSync;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/FileReaderSync/FileReaderSync
/// </summary>
public sealed class FileReaderSyncConstructor : Constructor
{
    internal FileReaderSyncConstructor(Engine engine)
        : base(engine, nameof(FileReaderSync))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private FileReaderSyncPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new FileReaderSyncInstance(Engine) { Prototype = PrototypeObject };
    }
}
