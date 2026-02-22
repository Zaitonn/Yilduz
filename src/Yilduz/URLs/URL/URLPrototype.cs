using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.URLs.URL;

internal sealed class URLPrototype : ObjectInstance
{
    public URLPrototype(Engine engine, URLConstructor ctor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(URL));
        SetOwnProperty("constructor", new(ctor, false, false, true));

        // Origin property (read-only)
        FastSetProperty(
            nameof(URLInstance.Origin).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(URLInstance.Origin).ToJsGetterName(),
                    GetOrigin
                ),
                set: null,
                false,
                true
            )
        );

        // Href property
        FastSetProperty(
            nameof(URLInstance.Href).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, nameof(URLInstance.Href).ToJsGetterName(), GetHref),
                set: new ClrFunction(engine, nameof(URLInstance.Href).ToJsSetterName(), SetHref),
                false,
                true
            )
        );

        // Protocol property
        FastSetProperty(
            nameof(URLInstance.Protocol).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(URLInstance.Protocol).ToJsGetterName(),
                    GetProtocol
                ),
                set: new ClrFunction(
                    engine,
                    nameof(URLInstance.Protocol).ToJsSetterName(),
                    SetProtocol
                ),
                false,
                true
            )
        );

        // Host property
        FastSetProperty(
            nameof(URLInstance.Host).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, nameof(URLInstance.Host).ToJsGetterName(), GetHost),
                set: new ClrFunction(engine, nameof(URLInstance.Host).ToJsSetterName(), SetHost),
                false,
                true
            )
        );

        // Hostname property
        FastSetProperty(
            nameof(URLInstance.Hostname).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(URLInstance.Hostname).ToJsGetterName(),
                    GetHostname
                ),
                set: new ClrFunction(
                    engine,
                    nameof(URLInstance.Hostname).ToJsSetterName(),
                    SetHostname
                ),
                false,
                true
            )
        );

        // Port property
        FastSetProperty(
            nameof(URLInstance.Port).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, nameof(URLInstance.Port).ToJsGetterName(), GetPort),
                set: new ClrFunction(engine, nameof(URLInstance.Port).ToJsSetterName(), SetPort),
                false,
                true
            )
        );

        // Pathname property
        FastSetProperty(
            nameof(URLInstance.Pathname).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(URLInstance.Pathname).ToJsGetterName(),
                    GetPathname
                ),
                set: new ClrFunction(
                    engine,
                    nameof(URLInstance.Pathname).ToJsSetterName(),
                    SetPathname
                ),
                false,
                true
            )
        );

        // Search property
        FastSetProperty(
            nameof(URLInstance.Search).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(URLInstance.Search).ToJsGetterName(),
                    GetSearch
                ),
                set: new ClrFunction(
                    engine,
                    nameof(URLInstance.Search).ToJsSetterName(),
                    SetSearch
                ),
                false,
                true
            )
        );

        // Hash property
        FastSetProperty(
            nameof(URLInstance.Hash).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, nameof(URLInstance.Hash).ToJsGetterName(), GetHash),
                set: new ClrFunction(engine, nameof(URLInstance.Hash).ToJsSetterName(), SetHash),
                false,
                true
            )
        );

        // Username property
        FastSetProperty(
            nameof(URLInstance.Username).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(URLInstance.Username).ToJsGetterName(),
                    GetUsername
                ),
                set: new ClrFunction(
                    engine,
                    nameof(URLInstance.Username).ToJsSetterName(),
                    SetUsername
                ),
                false,
                true
            )
        );

        // Password property
        FastSetProperty(
            nameof(URLInstance.Password).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(URLInstance.Password).ToJsGetterName(),
                    GetPassword
                ),
                set: new ClrFunction(
                    engine,
                    nameof(URLInstance.Password).ToJsSetterName(),
                    SetPassword
                ),
                false,
                true
            )
        );

        // SearchParams property (read-only)
        FastSetProperty(
            nameof(URLInstance.SearchParams).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(URLInstance.SearchParams).ToJsGetterName(),
                    GetSearchParams
                ),
                set: null,
                false,
                true
            )
        );

        // toString method
        FastSetProperty(
            "toString",
            new(new ClrFunction(Engine, "toString", ToString), false, false, true)
        );

        // toJSON method
        FastSetProperty(
            "toJSON",
            new(new ClrFunction(Engine, "toJSON", ToJSON), false, false, true)
        );
    }

    private static JsValue GetOrigin(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Origin;
    }

    private static JsValue GetHref(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Href;
    }

    private static JsValue SetHref(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        instance.Href = arguments.At(0).ToString();
        return Undefined;
    }

    private static JsValue GetProtocol(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Protocol;
    }

    private static JsValue SetProtocol(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        instance.Protocol = arguments.At(0).ToString();
        return Undefined;
    }

    private static JsValue GetHost(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Host;
    }

    private static JsValue SetHost(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        instance.Host = arguments.At(0).ToString();
        return Undefined;
    }

    private static JsValue GetHostname(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Hostname;
    }

    private static JsValue SetHostname(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        instance.Hostname = arguments.At(0).ToString();
        return Undefined;
    }

    private static JsValue GetPort(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Port;
    }

    private static JsValue SetPort(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        instance.Port = arguments.At(0).ToString();
        return Undefined;
    }

    private static JsValue GetPathname(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Pathname;
    }

    private static JsValue SetPathname(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        instance.Pathname = arguments.At(0).ToString();
        return Undefined;
    }

    private static JsValue GetSearch(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Search;
    }

    private static JsValue SetSearch(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        instance.Search = arguments.At(0).ToString();
        return Undefined;
    }

    private static JsValue GetHash(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Hash;
    }

    private static JsValue SetHash(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        instance.Hash = arguments.At(0).ToString();
        return Undefined;
    }

    private static JsValue GetUsername(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Username;
    }

    private static JsValue SetUsername(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        instance.Username = arguments.At(0).ToString();
        return Undefined;
    }

    private static JsValue GetPassword(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Password;
    }

    private static JsValue SetPassword(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        instance.Password = arguments.At(0).ToString();
        return Undefined;
    }

    private static JsValue GetSearchParams(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.SearchParams;
    }

    private static JsValue ToString(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Href;
    }

    private static JsValue ToJSON(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLInstance>();
        return instance.Href;
    }
}
