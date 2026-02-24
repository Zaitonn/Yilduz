using System;
using Jint;
using Jint.Native;
using Jint.Native.Json;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Network.Body;
using Yilduz.Network.Headers;
using Yilduz.URLs.URL;
using Yilduz.Utils;

namespace Yilduz.Network.Response;

internal sealed class ResponseConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    public ResponseConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(Response))
    {
        _webApiIntrinsics = webApiIntrinsics;
        PrototypeObject = new ResponsePrototype(Engine, this);

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));

        // static Response.error()
        FastSetProperty(
            "error",
            new PropertyDescriptor(new ClrFunction(engine, "error", StaticError), true, false, true)
        );

        // static Response.redirect(url, status = 302)
        FastSetProperty(
            "redirect",
            new PropertyDescriptor(
                new ClrFunction(engine, "redirect", StaticRedirect),
                true,
                false,
                true
            )
        );

        // static Response.json(data, init = {})
        FastSetProperty(
            "json",
            new PropertyDescriptor(new ClrFunction(engine, "json", StaticJson), true, false, true)
        );
    }

    internal ResponsePrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        var body = arguments.At(0);
        var init = arguments.At(1);

        return Create(body, init);
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#response-create
    /// </summary>
    internal ResponseInstance Create(ResponseConcept response, Guard guard)
    {
        var responseObject = new ResponseInstance(Engine, _webApiIntrinsics, response)
        {
            Prototype = PrototypeObject,
            Headers = _webApiIntrinsics.Headers.Construct(response.HeaderList, guard),
        };
        return responseObject;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-response
    /// </summary>
    private ResponseInstance Create(JsValue body, JsValue init)
    {
        // Set this's response to a new response.
        var response = new ResponseConcept();

        // Set this's headers to a new Headers object with this's relevant realm,
        // whose header list is this's response's header list and guard is "response".
        var responseObject = Create(response, Guard.Response);

        // Let bodyWithType be null.
        BodyWithType? bodyWithType = null;

        // If body is non-null, then set bodyWithType to the result of extracting body.
        if (!body.IsNull() && !body.IsUndefined())
        {
            bodyWithType = BodyExtractor.Extract(Engine, body);
        }

        // Perform initialize a response given this, init, and bodyWithType.
        InitializeResponse(responseObject, init, bodyWithType);

        return responseObject;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#initialize-a-response
    /// </summary>
    private void InitializeResponse(
        ResponseInstance responseObject,
        JsValue init,
        BodyWithType? body
    )
    {
        if (init.IsNull() || init.IsUndefined())
        {
            init = Engine.Intrinsics.Object.Construct(Arguments.Empty);
        }

        // 1. If init["status"] is not in the range 200 to 599, inclusive, then throw a RangeError.
        var statusJsValue = init.Get("status");
        ushort status = 200;
        if (!statusJsValue.IsUndefined())
        {
            var statusNum = (int)statusJsValue.AsNumber();
            if (statusNum is < 200 or > 599)
            {
                throw new JavaScriptException(
                    ErrorHelper.Create(
                        Engine,
                        "RangeError",
                        $"The status provided ({statusNum}) is outside the range [200, 599]."
                    )
                );
            }
            status = (ushort)statusNum;
        }

        // 2. If init["statusText"] is not the empty string and does not match the reason-phrase
        //    token production, then throw a TypeError.
        var statusTextJsValue = init.Get("statusText");
        var statusText = string.Empty;
        if (!statusTextJsValue.IsUndefined())
        {
            statusText = statusTextJsValue.AsString();
            if (!IsValidReasonPhrase(statusText))
            {
                TypeErrorHelper.Throw(
                    Engine,
                    $"Invalid statusText: '{statusText}' is not a valid reason phrase.",
                    "constructor",
                    "Response"
                );
            }
        }

        // 3. Set response's response's status to init["status"].
        responseObject.ResponseConcept.Status = status;

        // 4. Set response's response's status message to init["statusText"].
        responseObject.ResponseConcept.StatusMessage = statusText;

        // 5. If init["headers"] exists, then fill response's headers with init["headers"].
        var headersJsValue = init.Get("headers");
        if (!headersJsValue.IsUndefined())
        {
            responseObject.Headers.Fill(headersJsValue);
        }

        // 6. If body is non-null, then:
        if (body is not null)
        {
            // 6a. If response's status is a null body status, then throw a TypeError.
            if (IsNullBodyStatus(status))
            {
                TypeErrorHelper.Throw(
                    Engine,
                    "Response body cannot be set for status codes 101, 103, 204, 205, or 304.",
                    "constructor",
                    "Response"
                );
            }

            // 6b. Set response's body to body's body.
            responseObject.ResponseConcept.Body = body.Body;

            // 6c. If body's type is non-null and response's header list does not contain
            //     `Content-Type`, then append (`Content-Type`, body's type) to response's header list.
            if (!string.IsNullOrEmpty(body.Type) && !responseObject.Headers.Has("Content-Type"))
            {
                responseObject.Headers.Append("Content-Type", body.Type);
            }
        }
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-response-error
    /// <br/>
    /// The static error() method steps are to return the result of creating a Response object,
    /// given a new network error, "immutable", and the current realm.
    /// </summary>
    private JsValue StaticError(JsValue thisObject, JsValue[] arguments)
    {
        return Create(ResponseConcept.CreateNetworkError(), Guard.Immutable);
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-response-redirect
    /// </summary>
    private JsValue StaticRedirect(JsValue thisObject, JsValue[] arguments)
    {
        var url = arguments.At(0);
        var statusArg = arguments.At(1);

        // 1. Let parsedURL be the result of parsing url with current settings object's API base URL.
        //    If parsedURL is failure, then throw a TypeError.
        if (url.IsUndefined() || url.IsNull())
        {
            TypeErrorHelper.Throw(Engine, "URL is required.", "redirect", "Response");
        }

        URLInstance parsedUrl;
        try
        {
            parsedUrl = _webApiIntrinsics.URL.Parse(
                url.AsString(),
                _webApiIntrinsics.Options.BaseUrl?.ToString()
            );
        }
        catch (Exception e) when (e is not JavaScriptException)
        {
            TypeErrorHelper.Throw(Engine, "Invalid URL.", "redirect", "Response");
            return Undefined;
        }

        // 2. If status is not a redirect status, then throw a RangeError.
        var redirectStatus = statusArg.IsUndefined() ? (ushort)302 : (ushort)statusArg.AsNumber();
        if (!IsRedirectStatus(redirectStatus))
        {
            throw new JavaScriptException(
                ErrorHelper.Create(
                    Engine,
                    "RangeError",
                    $"Invalid redirect status {redirectStatus}. Must be 301, 302, 303, 307, or 308."
                )
            );
        }

        // 3. Let responseObject be the result of creating a Response object, given a new response,
        //    "immutable", and the current realm.
        var responseObject = Create(new(), Guard.Immutable);

        // 4. Set responseObject's response's status to status.
        responseObject.ResponseConcept.Status = redirectStatus;

        // 5. Let value be parsedURL, serialized and isomorphic encoded.
        // 6. Append (`Location`, value) to responseObject's response's header list.
        var locationValue = parsedUrl.Href;
        responseObject.ResponseConcept.HeaderList.Add(new("Location", locationValue));

        return responseObject;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-response-json
    /// </summary>
    private JsValue StaticJson(JsValue thisObject, JsValue[] arguments)
    {
        var data = arguments.At(0);
        var init = arguments.At(1);

        // 1. Let bytes be the result of running serialize a JavaScript value to JSON bytes on data.
        var stringifyFn = new JsonSerializer(Engine);
        var jsonStr = stringifyFn.Serialize(data).AsString();
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonStr);

        // 2. Let body be the result of extracting bytes.
        var bodyWithType = BodyExtractor.Extract(
            Engine,
            Engine.Intrinsics.Uint8Array.Construct(bytes)
        );

        // 3. Let responseObject be the result of creating a Response object, given a new response,
        //    "response", and the current realm.
        var responseObject = Create(new(), Guard.Response);

        // 4. Perform initialize a response given responseObject, init, and (body, "application/json").
        InitializeResponse(
            responseObject,
            init,
            new BodyWithType(bodyWithType.Body, "application/json")
        );

        return responseObject;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#null-body-status
    /// <br/>
    /// A null body status is a status that is 101, 103, 204, 205, or 304.
    /// </summary>
    private static bool IsNullBodyStatus(ushort status) =>
        status is 101 or 103 or 204 or 205 or 304;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#redirect-status
    /// <br/>
    /// A redirect status is a status that is 301, 302, 303, 307, or 308.
    /// </summary>
    private static bool IsRedirectStatus(ushort status) =>
        status is 301 or 302 or 303 or 307 or 308;

    /// <summary>
    /// https://httpwg.org/specs/rfc9112.html#status.line
    /// <br/>
    /// reason-phrase = *( HTAB / SP / VCHAR / obs-text )
    /// </summary>
    private static bool IsValidReasonPhrase(string statusText)
    {
        foreach (var c in statusText)
        {
            if (c == '\t' || c == ' ')
            {
                continue;
            }
            if (c >= 0x21 && c <= 0x7E)
            {
                continue; // VCHAR
            }
            if (c >= 0x80 && c <= 0xFF)
            {
                continue; // obs-text
            }
            return false;
        }
        return true;
    }
}
