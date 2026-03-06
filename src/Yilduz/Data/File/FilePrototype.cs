using Jint;
using Jint.Native;
using Yilduz.Models;

namespace Yilduz.Data.File;

internal sealed class FilePrototype : PrototypeBase<FileInstance>
{
    public FilePrototype(Engine engine, FileConstructor constructor)
        : base(engine, nameof(File), constructor)
    {
        RegisterProperty("lastModified", file => file.LastModified);
        RegisterProperty("lastModifiedDate", file => new JsDate(engine, file.LastModifiedDate));
        RegisterProperty("name", file => file.Name);
    }
}
