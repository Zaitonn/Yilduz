using Jint;
using Jint.Native;
using Yilduz.Network.Body;

namespace Yilduz.Network.Request;

internal sealed class RequestPrototype : BodyPrototype<RequestInstance>
{
    public RequestPrototype(Engine engine, RequestConstructor constructor)
        : base(engine, nameof(Request), constructor)
    {
        RegisterProperty("method", req => req.Method);
        RegisterProperty("url", req => req.Url);
        RegisterProperty("headers", req => req.Headers);
        RegisterProperty("destination", req => req.Destination);
        RegisterProperty("referrer", req => req.Referrer);
        RegisterProperty("referrerPolicy", req => req.ReferrerPolicy);
        RegisterProperty("mode", req => req.Mode);
        RegisterProperty("credentials", req => req.Credentials);
        RegisterProperty("cache", req => req.Cache);
        RegisterProperty("redirect", req => req.Redirect);
        RegisterProperty("integrity", req => req.Integrity);
        RegisterProperty("keepalive", req => req.Keepalive);
        RegisterProperty("isReloadNavigation", req => req.IsReloadNavigation);
        RegisterProperty("isHistoryNavigation", req => req.IsHistoryNavigation);
        RegisterProperty("signal", req => req.Signal);
        RegisterProperty("duplex", req => req.Duplex);
        RegisterProperty("body", req => (JsValue?)req.Body ?? Null);
        RegisterProperty("bodyUsed", req => req.BodyUsed);

        RegisterMethod("clone", (req, _) => req.Clone());
    }
}
