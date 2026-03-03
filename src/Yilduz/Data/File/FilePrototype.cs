using Jint;
using Yilduz.Models;

namespace Yilduz.Data.File;

internal sealed class FilePrototype : PrototypeBase<FileInstance>
{
    public FilePrototype(Engine engine, FileConstructor constructor)
        : base(engine, nameof(File), constructor)
    {
        RegisterProperty("lastModified", file => file.LastModified);
        RegisterProperty("name", file => file.Name);
    }
}
