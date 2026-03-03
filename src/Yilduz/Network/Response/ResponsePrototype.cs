using Jint;
using Jint.Native;
using Yilduz.Network.Body;

namespace Yilduz.Network.Response;

internal sealed class ResponsePrototype : BodyPrototype<ResponseInstance>
{
    public ResponsePrototype(Engine engine, ResponseConstructor constructor)
        : base(engine, nameof(Response), constructor)
    {
        RegisterProperty("type", res => res.Type);
        RegisterProperty("url", res => res.Url);
        RegisterProperty("redirected", res => res.Redirected);
        RegisterProperty("status", res => res.Status);
        RegisterProperty("ok", res => res.Ok);
        RegisterProperty("statusText", res => res.StatusText);
        RegisterProperty("headers", res => res.Headers);
        RegisterProperty("body", res => (JsValue?)res.Body ?? Null);
        RegisterProperty("bodyUsed", res => res.BodyUsed);

        RegisterMethod("clone", (res, _) => res.Clone());
    }
}
