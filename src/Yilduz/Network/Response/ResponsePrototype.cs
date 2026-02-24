using Jint;
using Jint.Native;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Network.Body;

namespace Yilduz.Network.Response;

internal sealed class ResponsePrototype : BodyPrototype
{
    private static readonly string TypeName = nameof(ResponseInstance.Type).ToJsStyleName();
    private static readonly string TypeGetterName = TypeName.ToJsGetterName();
    private static readonly string UrlName = nameof(ResponseInstance.Url).ToJsStyleName();
    private static readonly string UrlGetterName = UrlName.ToJsGetterName();
    private static readonly string RedirectedName = nameof(ResponseInstance.Redirected)
        .ToJsStyleName();
    private static readonly string RedirectedGetterName = RedirectedName.ToJsGetterName();
    private static readonly string StatusName = nameof(ResponseInstance.Status).ToJsStyleName();
    private static readonly string StatusGetterName = StatusName.ToJsGetterName();
    private static readonly string OkName = nameof(ResponseInstance.Ok).ToJsStyleName();
    private static readonly string OkGetterName = OkName.ToJsGetterName();
    private static readonly string StatusTextName = nameof(ResponseInstance.StatusText)
        .ToJsStyleName();
    private static readonly string StatusTextGetterName = StatusTextName.ToJsGetterName();
    private static readonly string HeadersName = nameof(ResponseInstance.Headers).ToJsStyleName();
    private static readonly string HeadersGetterName = HeadersName.ToJsGetterName();
    private static readonly string BodyName = nameof(ResponseInstance.Body).ToJsStyleName();
    private static readonly string BodyGetterName = BodyName.ToJsGetterName();
    private static readonly string BodyUsedName = nameof(ResponseInstance.BodyUsed).ToJsStyleName();
    private static readonly string BodyUsedGetterName = BodyUsedName.ToJsGetterName();
    public static readonly string CloneName = nameof(ResponseInstance.Clone).ToJsStyleName();

    public ResponsePrototype(Engine engine, ResponseConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(Response));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(
            TypeName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, TypeGetterName, GetType_),
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
            RedirectedName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, RedirectedGetterName, GetRedirected),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            StatusName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, StatusGetterName, GetStatus),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            OkName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OkGetterName, GetOk),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            StatusTextName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, StatusTextGetterName, GetStatusText),
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
            new PropertyDescriptor(new ClrFunction(engine, CloneName, Clone), false, false, true)
        );
    }

    private static JsValue GetType_(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ResponseInstance>().Type;
    }

    private static JsValue GetUrl(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ResponseInstance>().Url;
    }

    private static JsValue GetRedirected(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ResponseInstance>().Redirected;
    }

    private static JsValue GetStatus(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ResponseInstance>().Status;
    }

    private static JsValue GetOk(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ResponseInstance>().Ok;
    }

    private static JsValue GetStatusText(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ResponseInstance>().StatusText;
    }

    private static Headers.HeadersInstance GetHeaders(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ResponseInstance>().Headers;
    }

    private static JsValue GetBody(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ResponseInstance>().Body ?? Null;
    }

    private static JsValue GetBodyUsed(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ResponseInstance>().BodyUsed;
    }

    private static ResponseInstance Clone(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ResponseInstance>().Clone();
    }
}
