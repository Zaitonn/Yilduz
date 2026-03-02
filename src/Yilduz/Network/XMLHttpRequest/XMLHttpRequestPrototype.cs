using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.Network.XMLHttpRequest;

internal sealed class XMLHttpRequestPrototype : ObjectInstance
{
    private static readonly string OpenName = nameof(XMLHttpRequestInstance.Open).ToJsStyleName();
    private static readonly string SendName = nameof(XMLHttpRequestInstance.Send).ToJsStyleName();
    private static readonly string AbortName = nameof(XMLHttpRequestInstance.Abort).ToJsStyleName();
    private static readonly string SetRequestHeaderName = nameof(
            XMLHttpRequestInstance.SetRequestHeader
        )
        .ToJsStyleName();
    private static readonly string GetResponseHeaderName = nameof(
            XMLHttpRequestInstance.GetResponseHeader
        )
        .ToJsStyleName();
    private static readonly string GetAllResponseHeadersName = nameof(
            XMLHttpRequestInstance.GetAllResponseHeaders
        )
        .ToJsStyleName();
    private static readonly string ReadyStateName = nameof(XMLHttpRequestInstance.ReadyState)
        .ToJsStyleName();
    private static readonly string ResponseName = nameof(XMLHttpRequestInstance.Response)
        .ToJsStyleName();
    private static readonly string ResponseTextName = nameof(XMLHttpRequestInstance.ResponseText)
        .ToJsStyleName();
    private static readonly string ResponseTypeName = nameof(XMLHttpRequestInstance.ResponseType)
        .ToJsStyleName();
    private static readonly string ResponseURLName = nameof(XMLHttpRequestInstance.ResponseURL)
        .ToJsStyleName();
    private static readonly string ResponseXMLName = nameof(XMLHttpRequestInstance.ResponseXML)
        .ToJsStyleName();
    private static readonly string StatusName = nameof(XMLHttpRequestInstance.Status)
        .ToJsStyleName();
    private static readonly string StatusTextName = nameof(XMLHttpRequestInstance.StatusText)
        .ToJsStyleName();
    private static readonly string TimeoutName = nameof(XMLHttpRequestInstance.Timeout)
        .ToJsStyleName();
    private static readonly string WithCredentialsName = nameof(
            XMLHttpRequestInstance.WithCredentials
        )
        .ToJsStyleName();
    private static readonly string UploadName = nameof(XMLHttpRequestInstance.Upload)
        .ToJsStyleName();
    private static readonly string OverrideMimeTypeName = nameof(
            XMLHttpRequestInstance.OverrideMimeType
        )
        .ToJsStyleName();
    private static readonly string OnReadyStateChangeName = "onreadystatechange";

    public XMLHttpRequestPrototype(Engine engine, XMLHttpRequestConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(XMLHttpRequest));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(
            nameof(XMLHttpRequestReadyState.Unsent).ToUpperInvariant(),
            new((int)XMLHttpRequestReadyState.Unsent, false, false, false)
        );
        FastSetProperty(
            nameof(XMLHttpRequestReadyState.Opened).ToUpperInvariant(),
            new((int)XMLHttpRequestReadyState.Opened, false, false, false)
        );
        FastSetProperty(
            nameof(XMLHttpRequestReadyState.Headers_Received).ToUpperInvariant(),
            new((int)XMLHttpRequestReadyState.Headers_Received, false, false, false)
        );
        FastSetProperty(
            nameof(XMLHttpRequestReadyState.Loading).ToUpperInvariant(),
            new((int)XMLHttpRequestReadyState.Loading, false, false, false)
        );
        FastSetProperty(
            nameof(XMLHttpRequestReadyState.Done).ToUpperInvariant(),
            new((int)XMLHttpRequestReadyState.Done, false, false, false)
        );

        FastSetProperty(OpenName, new(new ClrFunction(Engine, OpenName, Open), false, false, true));
        FastSetProperty(SendName, new(new ClrFunction(Engine, SendName, Send), false, false, true));
        FastSetProperty(
            AbortName,
            new(new ClrFunction(Engine, AbortName, Abort), false, false, true)
        );
        FastSetProperty(
            SetRequestHeaderName,
            new(new ClrFunction(Engine, SetRequestHeaderName, SetRequestHeader), false, false, true)
        );
        FastSetProperty(
            GetResponseHeaderName,
            new(
                new ClrFunction(Engine, GetResponseHeaderName, GetResponseHeader),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            GetAllResponseHeadersName,
            new(
                new ClrFunction(Engine, GetAllResponseHeadersName, GetAllResponseHeaders),
                false,
                false,
                true
            )
        );

        FastSetProperty(
            OnReadyStateChangeName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    Engine,
                    OnReadyStateChangeName.ToJsGetterName(),
                    GetOnReadyStateChange
                ),
                set: new ClrFunction(
                    Engine,
                    OnReadyStateChangeName.ToJsSetterName(),
                    SetOnReadyStateChange
                ),
                false,
                true
            )
        );

        FastSetProperty(
            ReadyStateName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, ReadyStateName.ToJsGetterName(), GetReadyState),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            ResponseName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, ResponseName.ToJsGetterName(), GetResponse),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            ResponseTextName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, ResponseTextName.ToJsGetterName(), GetResponseText),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            ResponseTypeName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, ResponseTypeName.ToJsGetterName(), GetResponseType),
                set: new ClrFunction(Engine, ResponseTypeName.ToJsSetterName(), SetResponseType),
                false,
                true
            )
        );

        FastSetProperty(
            ResponseURLName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, ResponseURLName.ToJsGetterName(), GetResponseURL),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            ResponseXMLName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, ResponseXMLName.ToJsGetterName(), GetResponseXML),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            StatusName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, StatusName.ToJsGetterName(), GetStatus),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            StatusTextName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, StatusTextName.ToJsGetterName(), GetStatusText),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            TimeoutName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, TimeoutName.ToJsGetterName(), GetTimeout),
                set: new ClrFunction(Engine, TimeoutName.ToJsSetterName(), SetTimeout),
                false,
                true
            )
        );

        FastSetProperty(
            WithCredentialsName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    Engine,
                    WithCredentialsName.ToJsGetterName(),
                    GetWithCredentials
                ),
                set: new ClrFunction(
                    Engine,
                    WithCredentialsName.ToJsSetterName(),
                    SetWithCredentials
                ),
                false,
                true
            )
        );

        FastSetProperty(
            UploadName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, UploadName.ToJsGetterName(), GetUpload),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            OverrideMimeTypeName,
            new(new ClrFunction(Engine, OverrideMimeTypeName, OverrideMimeType), false, false, true)
        );
    }

    private JsValue Open(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 2, OpenName, nameof(XMLHttpRequest));

        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        var method = arguments[0].ToString();
        var url = arguments[1].ToString();

        var async =
            arguments.Length < 3 || arguments[2].IsUndefined() || arguments[2].ConvertToBoolean();
        var user =
            arguments.Length > 3 && !arguments[3].IsUndefined() ? arguments[3].ToString() : null;
        var password =
            arguments.Length > 4 && !arguments[4].IsUndefined() ? arguments[4].ToString() : null;

        instance.Open(method, url, async, user, password);
        return Undefined;
    }

    private JsValue Send(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        var body = arguments.At(0);
        instance.Send(body);
        return Undefined;
    }

    private JsValue Abort(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        instance.Abort();
        return Undefined;
    }

    private JsValue SetRequestHeader(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 2, SetRequestHeaderName, nameof(XMLHttpRequest));

        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        var name = arguments[0].ToString();
        var value = arguments[1].ToString();
        instance.SetRequestHeader(name, value);
        return Undefined;
    }

    private JsValue GetResponseHeader(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, GetResponseHeaderName, nameof(XMLHttpRequest));

        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        var name = arguments[0].ToString();
        var value = instance.GetResponseHeader(name);
        return value is null ? Null : value;
    }

    private JsValue GetAllResponseHeaders(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        return instance.GetAllResponseHeaders();
    }

    private JsValue GetOnReadyStateChange(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        return instance.OnReadyStateChange;
    }

    private JsValue SetOnReadyStateChange(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        instance.OnReadyStateChange = arguments.At(0);
        return instance.OnReadyStateChange;
    }

    private JsValue GetReadyState(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        return (int)instance.ReadyState;
    }

    private JsValue GetResponse(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        return instance.Response;
    }

    private JsValue GetResponseText(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        var responseType = instance.ResponseType ?? string.Empty;
        if (responseType is not ("" or "text"))
        {
            DOMExceptionHelper
                .CreateInvalidStateError(
                    Engine,
                    "The object's 'responseType' is 'arraybuffer'/'blob'/'json'. Use 'response' instead of 'responseText'."
                )
                .Throw();
        }
        return instance.ResponseText;
    }

    private JsValue GetResponseType(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        return instance.ResponseType;
    }

    private JsValue SetResponseType(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        instance.ResponseType = arguments.At(0).ToString();
        return instance.ResponseType;
    }

    private JsValue GetResponseURL(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        return instance.ResponseURL;
    }

    private JsValue GetResponseXML(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        return instance.ResponseXML;
    }

    private JsValue GetStatus(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        return (int)instance.Status;
    }

    private JsValue GetStatusText(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        return instance.StatusText;
    }

    private JsValue GetTimeout(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        return instance.Timeout;
    }

    private JsValue SetTimeout(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        instance.Timeout = (long)arguments.At(0).AsNumber();
        return instance.Timeout;
    }

    private JsValue GetWithCredentials(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        return instance.WithCredentials;
    }

    private JsValue SetWithCredentials(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        instance.WithCredentials = arguments.At(0).ConvertToBoolean();
        return instance.WithCredentials;
    }

    private JsValue GetUpload(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        return instance.Upload;
    }

    private JsValue OverrideMimeType(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, OverrideMimeTypeName, nameof(XMLHttpRequest));

        var instance = thisObject.EnsureThisObject<XMLHttpRequestInstance>();
        instance.OverrideMimeType(arguments[0].ToString());
        return Undefined;
    }
}
