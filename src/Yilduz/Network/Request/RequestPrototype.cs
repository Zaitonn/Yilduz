using Jint;
using Jint.Native;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Network.Body;

namespace Yilduz.Network.Request;

internal sealed class RequestPrototype : BodyPrototype
{
    private static readonly string MethodName = nameof(RequestInstance.Method).ToJsStyleName();
    private static readonly string MethodGetterName = MethodName.ToJsGetterName();
    private static readonly string UrlName = nameof(RequestInstance.Url).ToJsStyleName();
    private static readonly string UrlGetterName = UrlName.ToJsGetterName();
    private static readonly string HeadersName = nameof(RequestInstance.Headers).ToJsStyleName();
    private static readonly string HeadersGetterName = HeadersName.ToJsGetterName();
    private static readonly string DestinationName = nameof(RequestInstance.Destination)
        .ToJsStyleName();
    private static readonly string DestinationGetterName = DestinationName.ToJsGetterName();
    private static readonly string ReferrerName = nameof(RequestInstance.Referrer).ToJsStyleName();
    private static readonly string ReferrerGetterName = ReferrerName.ToJsGetterName();
    private static readonly string ReferrerPolicyName = nameof(RequestInstance.ReferrerPolicy)
        .ToJsStyleName();
    private static readonly string ReferrerPolicyGetterName = ReferrerPolicyName.ToJsGetterName();
    private static readonly string ModeName = nameof(RequestInstance.Mode).ToJsStyleName();
    private static readonly string ModeGetterName = ModeName.ToJsGetterName();
    private static readonly string CredentialsName = nameof(RequestInstance.Credentials)
        .ToJsStyleName();
    private static readonly string CredentialsGetterName = CredentialsName.ToJsGetterName();
    private static readonly string CacheName = nameof(RequestInstance.Cache).ToJsStyleName();
    private static readonly string CacheGetterName = CacheName.ToJsGetterName();
    private static readonly string RedirectName = nameof(RequestInstance.Redirect).ToJsStyleName();
    private static readonly string RedirectGetterName = RedirectName.ToJsGetterName();
    private static readonly string IntegrityName = nameof(RequestInstance.Integrity)
        .ToJsStyleName();
    private static readonly string IntegrityGetterName = IntegrityName.ToJsGetterName();
    private static readonly string KeepaliveName = nameof(RequestInstance.Keepalive)
        .ToJsStyleName();
    private static readonly string KeepaliveGetterName = KeepaliveName.ToJsGetterName();
    private static readonly string ReloadNavigationName = nameof(RequestInstance.IsReloadNavigation)
        .ToJsStyleName();
    private static readonly string ReloadNavigationGetterName =
        ReloadNavigationName.ToJsGetterName();
    private static readonly string HistoryNavigationName = nameof(
            RequestInstance.IsHistoryNavigation
        )
        .ToJsStyleName();
    private static readonly string HistoryNavigationGetterName =
        HistoryNavigationName.ToJsGetterName();
    private static readonly string SignalName = nameof(RequestInstance.Signal).ToJsStyleName();
    private static readonly string SignalGetterName = SignalName.ToJsGetterName();
    private static readonly string DuplexName = nameof(RequestInstance.Duplex).ToJsStyleName();
    private static readonly string DuplexGetterName = DuplexName.ToJsGetterName();
    private static readonly string BodyName = nameof(RequestInstance.Body).ToJsStyleName();
    private static readonly string BodyGetterName = BodyName.ToJsGetterName();
    private static readonly string BodyUsedName = nameof(RequestInstance.BodyUsed).ToJsStyleName();
    private static readonly string BodyUsedGetterName = BodyUsedName.ToJsGetterName();
    private static readonly string CloneName = nameof(RequestInstance.Clone).ToJsStyleName();

    public RequestPrototype(Engine engine, RequestConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(Request));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(
            MethodName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, MethodGetterName, GetMethod),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            UrlName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, UrlGetterName, GetUrl),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            HeadersName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, HeadersGetterName, GetHeaders),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            DestinationName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, DestinationGetterName, GetDestination),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            ReferrerName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ReferrerGetterName, GetReferrer),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            ReferrerPolicyName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ReferrerPolicyGetterName, GetReferrerPolicy),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            ModeName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ModeGetterName, GetMode),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            CredentialsName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, CredentialsGetterName, GetCredentials),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            CacheName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, CacheGetterName, GetCache),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            RedirectName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, RedirectGetterName, GetRedirect),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            IntegrityName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, IntegrityGetterName, GetIntegrity),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            KeepaliveName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, KeepaliveGetterName, GetKeepalive),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            ReloadNavigationName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ReloadNavigationGetterName, GetReloadNavigation),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            HistoryNavigationName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, HistoryNavigationGetterName, GetHistoryNavigation),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            SignalName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, SignalGetterName, GetSignal),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            DuplexName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, DuplexGetterName, GetDuplex),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            BodyName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, BodyGetterName, GetBody),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            BodyUsedName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, BodyUsedGetterName, GetBodyUsed),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            CloneName,
            new(new ClrFunction(engine, CloneName, Clone), false, false, true)
        );
    }

    private static JsValue GetMethod(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Method;
    }

    private static JsValue GetUrl(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Url;
    }

    private static Headers.HeadersInstance GetHeaders(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Headers;
    }

    private static JsValue GetDestination(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Destination;
    }

    private static JsValue GetReferrer(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Referrer;
    }

    private static JsValue GetReferrerPolicy(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().ReferrerPolicy;
    }

    private static JsValue GetMode(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Mode;
    }

    private static JsValue GetCredentials(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Credentials;
    }

    private static JsValue GetCache(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Cache;
    }

    private static JsValue GetRedirect(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Redirect;
    }

    private static JsValue GetIntegrity(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Integrity;
    }

    private static JsValue GetKeepalive(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Keepalive;
    }

    private static JsValue GetReloadNavigation(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().IsReloadNavigation;
    }

    private static JsValue GetHistoryNavigation(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().IsHistoryNavigation;
    }

    private static JsValue GetSignal(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Signal ?? Null;
    }

    private static JsValue GetDuplex(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Duplex;
    }

    private static JsValue GetBody(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Body ?? Null;
    }

    private static JsValue GetBodyUsed(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().BodyUsed;
    }

    private static RequestInstance Clone(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<RequestInstance>().Clone();
    }
}
