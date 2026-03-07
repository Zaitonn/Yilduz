using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Extensions;
using Yilduz.Models;
using Yilduz.Utils;

namespace Yilduz.Network.XMLHttpRequest;

internal sealed class XMLHttpRequestPrototype : PrototypeBase<XMLHttpRequestInstance>
{
    public XMLHttpRequestPrototype(Engine engine, XMLHttpRequestConstructor constructor)
        : base(engine, nameof(XMLHttpRequest), constructor)
    {
        RegisterConstant(
            nameof(XMLHttpRequestReadyState.UNSENT).ToUpperInvariant(),
            XMLHttpRequestReadyState.UNSENT
        );
        RegisterConstant(
            nameof(XMLHttpRequestReadyState.OPENED).ToUpperInvariant(),
            XMLHttpRequestReadyState.OPENED
        );
        RegisterConstant(
            nameof(XMLHttpRequestReadyState.HEADERS_RECEIVED).ToUpperInvariant(),
            XMLHttpRequestReadyState.HEADERS_RECEIVED
        );
        RegisterConstant(
            nameof(XMLHttpRequestReadyState.LOADING).ToUpperInvariant(),
            XMLHttpRequestReadyState.LOADING
        );
        RegisterConstant(
            nameof(XMLHttpRequestReadyState.DONE).ToUpperInvariant(),
            XMLHttpRequestReadyState.DONE
        );

        RegisterMethod("open", Open, 2);
        RegisterMethod("send", Send);
        RegisterMethod("abort", Abort);
        RegisterMethod("setRequestHeader", SetRequestHeader, 2);
        RegisterMethod("getResponseHeader", GetResponseHeader, 1);
        RegisterMethod("getAllResponseHeaders", GetAllResponseHeaders);
        RegisterMethod("overrideMimeType", OverrideMimeType, 1);

        RegisterProperty("onreadystatechange", GetOnReadyStateChange, SetOnReadyStateChange);
        RegisterProperty("readyState", xhr => (int)xhr.ReadyState);
        RegisterProperty("response", xhr => xhr.Response);
        RegisterProperty("responseText", GetResponseText);
        RegisterProperty("responseType", GetResponseType, SetResponseType);
        RegisterProperty("responseURL", xhr => xhr.ResponseURL);
        RegisterProperty("responseXML", xhr => xhr.ResponseXML);
        RegisterProperty("status", xhr => (int)xhr.Status);
        RegisterProperty("statusText", xhr => xhr.StatusText);
        RegisterProperty("timeout", GetTimeout, SetTimeout);
        RegisterProperty("withCredentials", GetWithCredentials, SetWithCredentials);
        RegisterProperty("upload", xhr => xhr.Upload);
    }

    private JsValue Open(XMLHttpRequestInstance instance, JsValue[] arguments)
    {
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

    private static JsValue Send(XMLHttpRequestInstance instance, JsValue[] arguments)
    {
        instance.Send(arguments.At(0));
        return Undefined;
    }

    private static JsValue Abort(XMLHttpRequestInstance instance, JsValue[] arguments)
    {
        instance.Abort();
        return Undefined;
    }

    private static JsValue SetRequestHeader(XMLHttpRequestInstance instance, JsValue[] arguments)
    {
        var name = arguments[0].ToString();
        var value = arguments[1].ToString();
        instance.SetRequestHeader(name, value);
        return Undefined;
    }

    private static JsValue GetResponseHeader(XMLHttpRequestInstance instance, JsValue[] arguments)
    {
        var name = arguments[0].ToString();
        var value = instance.GetResponseHeader(name);
        return value is null ? Null : value;
    }

    private static JsValue GetAllResponseHeaders(
        XMLHttpRequestInstance instance,
        JsValue[] arguments
    )
    {
        return instance.GetAllResponseHeaders();
    }

    private static JsValue OverrideMimeType(XMLHttpRequestInstance instance, JsValue[] arguments)
    {
        instance.OverrideMimeType(arguments[0].ToString());
        return Undefined;
    }

    private static JsValue GetOnReadyStateChange(XMLHttpRequestInstance instance)
    {
        return instance.OnReadyStateChange;
    }

    private static JsValue SetOnReadyStateChange(XMLHttpRequestInstance instance, JsValue argument)
    {
        instance.OnReadyStateChange = argument;
        return instance.OnReadyStateChange;
    }

    private JsValue GetResponseText(XMLHttpRequestInstance instance)
    {
        var responseType = instance.ResponseType;
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

    private static JsValue GetResponseType(XMLHttpRequestInstance instance)
    {
        return instance.ResponseType;
    }

    private static JsValue SetResponseType(XMLHttpRequestInstance instance, JsValue argument)
    {
        instance.ResponseType = argument.ToString();
        return instance.ResponseType;
    }

    private static JsValue GetTimeout(XMLHttpRequestInstance instance)
    {
        return instance.Timeout;
    }

    private static JsValue SetTimeout(XMLHttpRequestInstance instance, JsValue argument)
    {
        instance.Timeout = (long)argument.AsNumber();
        return instance.Timeout;
    }

    private static JsValue GetWithCredentials(XMLHttpRequestInstance instance)
    {
        return instance.WithCredentials;
    }

    private static JsValue SetWithCredentials(XMLHttpRequestInstance instance, JsValue argument)
    {
        instance.WithCredentials = argument.ConvertToBoolean();
        return instance.WithCredentials;
    }
}
