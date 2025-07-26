using Jint;
using Jint.Native.Object;

namespace Yilduz.Data.URL;

public sealed class URLInstance : ObjectInstance
{
    internal string? Query { get; set; }

    internal URLInstance(Engine engine)
        : base(engine) { }
}
