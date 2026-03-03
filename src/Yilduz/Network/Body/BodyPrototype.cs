using Jint;
using Jint.Native;
using Yilduz.Models;

namespace Yilduz.Network.Body;

internal abstract class BodyPrototype<T> : PrototypeBase<T>
    where T : BodyInstance
{
    protected BodyPrototype(Engine engine, string name, Constructor constructor)
        : base(engine, name, constructor)
    {
        RegisterMethod("arrayBuffer", (body, _) => body.ArrayBuffer());
        RegisterMethod("blob", (body, _) => body.Blob());
        RegisterMethod("formData", (body, _) => body.FormData());
        RegisterMethod("json", (body, _) => body.Json());
        RegisterMethod("text", (body, _) => body.Text());
        RegisterMethod("bytes", (body, _) => body.Bytes());
    }
}
