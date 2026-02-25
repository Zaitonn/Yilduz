using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Extensions;
using Yilduz.Network.Body;
using Yilduz.Network.Headers;
using Yilduz.Utils;

namespace Yilduz.Network.Request;

internal sealed class RequestConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    public RequestConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(Request))
    {
        _webApiIntrinsics = webApiIntrinsics;
        PrototypeObject = new RequestPrototype(Engine, this);

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    internal RequestPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'Request'");

        var input = arguments.At(0);
        var init = arguments.At(1);

        return Create(input, init);
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#request-create
    /// </summary>
    private RequestInstance Create(
        RequestConcept request,
        Guard guard,
        AbortSignalInstance abortSignal
    )
    {
        var requestObject = new RequestInstance(Engine, _webApiIntrinsics, request)
        {
            Prototype = PrototypeObject,
            Headers = _webApiIntrinsics.Headers.Construct(request.HeaderList, guard),
            Signal = abortSignal,
        };
        return requestObject;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-request
    /// </summary>
    internal RequestInstance Create(JsValue input, JsValue init)
    {
        var headerList = new List<(string Name, string Value)>();

        // Let request be null.
        RequestConcept? request = null;

        // Let fallbackMode be null.
        string? fallbackMode = null;

        // Let baseURL be this’s relevant settings object’s API base URL.
        var baseUrl = _webApiIntrinsics.Options.BaseUrl;

        // Let signal be null.
        AbortSignalInstance? signal = null;

        // If input is a string, then:
        if (input.IsString())
        {
            try
            {
                // Let parsedURL be the result of parsing input with baseURL.
                var parsedUrl = _webApiIntrinsics.URL.Parse(input.AsString(), baseUrl?.ToString());

                // If parsedURL is failure, then throw a TypeError.
                // If parsedURL includes credentials, then throw a TypeError.
                if (
                    parsedUrl.IsNull()
                    || !string.IsNullOrEmpty(parsedUrl.Username)
                    || !string.IsNullOrEmpty(parsedUrl.Password)
                )
                {
                    TypeErrorHelper.Throw(
                        Engine,
                        "Invalid URL provided to Request().",
                        "constructor",
                        nameof(Request)
                    );
                }

                // Set request to a new request whose URL is parsedURL.
                // request = new() { Url = parsedUrl };
                request = new() { URLList = [parsedUrl] };

                // Set fallbackMode to "cors".
                fallbackMode = Mode.CORS;
            }
            catch (Exception e) when (e is not JavaScriptException)
            {
                TypeErrorHelper.Throw(
                    Engine,
                    "Invalid URL provided to Request().",
                    "constructor",
                    nameof(Request)
                );
            }
        }
        else
        {
            // Assert: input is a Request object.
            if (input is not RequestInstance requestInstance1)
            {
                TypeErrorHelper.Throw(
                    Engine,
                    "Input must be a string or a Request object.",
                    "constructor",
                    nameof(Request)
                );

                return null;
            }

            // Set request to input’s request.
            request = requestInstance1.RequestConcept;

            // Set signal to input’s signal.
            signal = requestInstance1.Signal;
        }

        // Let origin be this’s relevant settings object’s origin.
        var origin = _webApiIntrinsics.Options.BaseUrl is null
            ? null
            : $"{_webApiIntrinsics.Options.BaseUrl.Scheme}://{_webApiIntrinsics.Options.BaseUrl.Authority}";

        // Let traversableForUserPrompts be "client".
        var traversableForUserPrompts = "client";

        // If request’s traversable for user prompts is an environment settings object and its origin is same origin with origin, then set traversableForUserPrompts to request’s traversable for user prompts.

        // If init["window"] exists and is non-null, then throw a TypeError.
        var window = init.Get("window");
        if (!window.IsUndefined() && !window.IsNull())
        {
            TypeErrorHelper.Throw(
                Engine,
                "Member window must be null.",
                "constructor",
                nameof(Request)
            );
        }
        else if (!window.IsUndefined())
        {
            // If init["window"] exists, then set traversableForUserPrompts to "no-traversable".
            traversableForUserPrompts = "no-traversable";
        }

        // Set request to a new request with the following properties:
        request = new()
        {
            // Url = request.Url,
            Method = request.Method,
            HeaderList = [.. request.HeaderList],
            UnsafeRequestFlag = true,
            // Client = ?,
            TraversableForUserPrompts = traversableForUserPrompts,
            InternalPriority = request.InternalPriority,
            Origin = request.Origin,
            Referrer = request.Referrer,
            ReferrerPolicy = request.ReferrerPolicy,
            Mode = request.Mode,
            CredentialsMode = request.CredentialsMode,
            CacheMode = request.CacheMode,
            RedirectMode = request.RedirectMode,
            IntegrityMetadata = request.IntegrityMetadata,
            Keepalive = request.Keepalive,
            ReloadNavigationFlag = request.ReloadNavigationFlag,
            HistoryNavigationFlag = request.HistoryNavigationFlag,
            URLList = [.. request.URLList],
            InitiatorType = InitiatorType.Fetch,
        };

        // If init is not empty, then:
        if (!init.IsNull() && !init.IsUndefined())
        {
            // If request’s mode is "navigate", then set it to "same-origin".
            if (request.Mode == Mode.Navigate)
            {
                request.Mode = Mode.SameOrigin;
            }

            // Set request’s origin to "client".
            request.Origin = origin ?? "client";

            // Set request’s referrer to "client".
            request.Referrer = "client";

            // Set request’s referrer policy to the empty string.
            request.ReferrerPolicy = string.Empty;

            // Set request’s URL to request’s current URL.
            // request.Url = request.CurrentURL;

            // Set request’s URL list to « request’s URL ».
            request.URLList.Add(request.Url);
        }

        #region referrer
        // If init["referrer"] exists, then:
        if (!init.Get("referrer").IsUndefined())
        {
            // Let referrer be init["referrer"].
            var referrer = init.Get("referrer").AsString();

            // If referrer is the empty string, then set request’s referrer to "no-referrer".
            if (string.IsNullOrEmpty(referrer))
            {
                request.Referrer = "no-referrer";
            }
            else
            {
                // Let parsedReferrer be the result of parsing referrer with baseURL.
                try
                {
                    var parsedReferrer = _webApiIntrinsics.URL.Parse(referrer, baseUrl?.ToString());

                    // If one of the following is true
                    //  parsedReferrer’s scheme is "about" and path is the string "client"
                    //  parsedReferrer’s origin is not same origin with origin
                    // then set request’s referrer to "client".
                    if (
                        (parsedReferrer.Protocol == "about:" && parsedReferrer.Pathname == "client")
                        || (origin is not null && parsedReferrer.Origin != origin)
                    )
                    {
                        request.Referrer = "client";
                    }
                    else
                    {
                        // Otherwise, set request’s referrer to parsedReferrer.
                        request.Referrer = parsedReferrer;
                    }
                }
                catch (Exception e) when (e is not JavaScriptException)
                {
                    // If parsedReferrer is failure, then throw a TypeError.
                    TypeErrorHelper.Throw(
                        Engine,
                        "Failed to parse referrer.",
                        "constructor",
                        nameof(Request)
                    );
                }
            }
        }
        #endregion

        #region referrerPolicy
        // If init["referrerPolicy"] exists, then set request’s referrer policy to it.
        if (!init.Get("referrerPolicy").IsUndefined())
        {
            request.ReferrerPolicy = init.Get("referrerPolicy").ToString();
        }
        #endregion

        #region mode
        // Let mode be init["mode"] if it exists, and fallbackMode otherwise.
        string? mode = null;

        if (!init.Get("mode").IsUndefined())
        {
            mode = init.Get("mode").ToString();
        }
        else if (fallbackMode is not null)
        {
            mode = fallbackMode;
        }

        // If mode is "navigate", then throw a TypeError.
        if (mode == Mode.Navigate)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Request mode 'navigate' is not allowed.",
                "constructor",
                nameof(Request)
            );
        }

        // If mode is non-null, set request’s mode to mode.
        if (!string.IsNullOrEmpty(mode))
        {
            request.Mode = mode;
        }
        #endregion

        #region credentials
        // If init["credentials"] exists, then set request’s credentials mode to it.
        if (!init.Get("credentials").IsUndefined())
        {
            request.CredentialsMode = init.Get("credentials").ToString();
        }
        #endregion

        #region cache
        // If init["cache"] exists, then set request’s cache mode to it.
        if (!init.Get("cache").IsUndefined())
        {
            request.CacheMode = init.Get("cache").ToString();
        }

        // If request’s cache mode is "only-if-cached" and request’s mode is not "same-origin", then throw a TypeError.
        if (request.CacheMode == Cache.OnlyIfCached && request.Mode != Mode.SameOrigin)
        {
            TypeErrorHelper.Throw(
                Engine,
                "cache 'only-if-cached' requires mode 'same-origin'.",
                "constructor",
                nameof(Request)
            );
        }
        #endregion

        #region redirect
        // If init["redirect"] exists, then set request’s redirect mode to it.
        if (!init.Get("redirect").IsUndefined())
        {
            request.RedirectMode = init.Get("redirect").ToString();
        }
        #endregion

        #region integrity
        // If init["integrity"] exists, then set request’s integrity metadata to it.
        if (!init.Get("integrity").IsUndefined())
        {
            request.IntegrityMetadata = init.Get("integrity").ToString();
        }
        #endregion

        #region keepalive
        // If init["keepalive"] exists, then set request’s keepalive to it.
        if (!init.Get("keepalive").IsUndefined())
        {
            request.Keepalive = init.Get("keepalive").AsBoolean();
        }
        #endregion

        #region method
        // If init["method"] exists, then:
        //  Let method be init["method"].
        //  If method is not a method or method is a forbidden method, then throw a TypeError.
        //  Normalize method.
        //  Set request’s method to method.
        if (!init.Get("method").IsUndefined())
        {
            var method = init.Get("method").ToString();

            if (!IsValidMethod(method) || HttpHelper.IsForbiddenMethod(method))
            {
                TypeErrorHelper.Throw(
                    Engine,
                    "Invalid or forbidden request method.",
                    "constructor",
                    nameof(Request)
                );
            }

            request.Method = NormalizeMethod(method);
        }
        #endregion

        #region signal
        // If init["signal"] exists, then set request’s signal to it.
        if (!init.Get("signal").IsUndefined())
        {
            var signalValue = init.Get("signal").TryCast<AbortSignalInstance>();

            if (signalValue is null)
            {
                TypeErrorHelper.Throw(
                    Engine,
                    "Failed to convert value to 'AbortSignal'.",
                    "constructor",
                    nameof(Request)
                );
            }

            signal = signalValue;
        }
        #endregion

        #region priority
        // If init["priority"] exists, then:
        // If request’s internal priority is not null, then update request’s internal priority in an implementation-defined manner.
        // Otherwise, set request’s priority to init["priority"].
        if (!init.Get("priority").IsUndefined()) { }
        #endregion

        // Set this’s request to request.
        // Let signals be « signal » if signal is non-null; otherwise « ».
        var signals = signal is not null ? new[] { signal } : [];

        // Set this’s signal to the result of creating a dependent abort signal from signals, using AbortSignal and this’s relevant realm.
        // Set this’s headers to a new Headers object with this’s relevant realm, whose header list is request’s header list and guard is "request".
        var requestInstance = Create(
            request,
            Guard.Request,
            _webApiIntrinsics.AbortSignal.CreateDependentSignal(signal)
        );

        // If this’s request’s mode is "no-cors", then:
        if (requestInstance.RequestConcept.Mode == Mode.NoCors)
        {
            // If this’s request’s method is not a CORS-safelisted method, then throw a TypeError.
            if (!IsCorsSafelistedMethod(requestInstance.RequestConcept.Method))
            {
                TypeErrorHelper.Throw(
                    Engine,
                    "CORS-safelisted methods are only allowed in no-cors mode.",
                    "constructor",
                    nameof(Request)
                );
            }
            // Set this’s headers’s guard to "request-no-cors".
            requestInstance.Headers.Guard = Guard.RequestNoCors;
        }

        // If init is not empty, then:
        if (!init.IsNull() && !init.IsUndefined())
        {
            // Let headers be a copy of this’s headers and its associated header list.
            JsValue headers = requestInstance.Headers;

            // If init["headers"] exists, then set headers to init["headers"].
            if (!init.Get("headers").IsUndefined())
            {
                headers = init.Get("headers");
            }

            // Empty this’s headers’s header list.
            requestInstance.RequestConcept.HeaderList.Clear();

            // If headers is a Headers object, then for each header of its header list, append header to this’s headers.
            if (headers is HeadersInstance headersInstance)
            {
                foreach (var (name, value) in headersInstance.HeaderList)
                {
                    requestInstance.Headers.Append(name, value);
                }
            }
            else
            {
                // Otherwise, fill this’s headers with headers.
                requestInstance.Headers.Fill(headers);
            }
        }

        // Let inputBody be input’s request’s body if input is a Request object; otherwise null.
        var inputBody = input is RequestInstance req ? req.BodyConcept : null;

        // If either init["body"] exists and is non-null or inputBody is non-null, and request’s method is `GET` or `HEAD`, then throw a TypeError.
        if (
            requestInstance.RequestConcept.Method == "GET"
            || requestInstance.RequestConcept.Method == "HEAD"
        )
        {
            if (
                !init.Get("body").IsUndefined() && !init.Get("body").IsNull()
                || inputBody is not null
            )
            {
                TypeErrorHelper.Throw(
                    Engine,
                    "Request body cannot be specified for GET or HEAD requests.",
                    "constructor",
                    nameof(Request)
                );
            }
        }

        // Let initBody be null.
        BodyConcept? initBody = null;

        // If init["body"] exists and is non-null, then:
        if (!init.Get("body").IsUndefined() && !init.Get("body").IsNull())
        {
            // Let bodyWithType be the result of extracting init["body"], with keepalive set to request’s keepalive.
            var bodyWithType = BodyExtractor.Extract(
                Engine,
                init.Get("body"),
                requestInstance.Keepalive
            );

            // Set initBody to bodyWithType’s body.
            initBody = bodyWithType.Body;

            // Let type be bodyWithType’s type.
            var type = bodyWithType.Type;

            // If type is non-null and this’s headers’s header list does not contain `Content-Type`, then append (`Content-Type`, type) to this’s headers.
            if (!string.IsNullOrEmpty(type) && !requestInstance.Headers.Has("Content-Type"))
            {
                requestInstance.Headers.Append("Content-Type", type);
            }
        }

        // Let inputOrInitBody be initBody if it is non-null; otherwise inputBody.
        var inputOrInitBody = initBody ?? inputBody;

        // If inputOrInitBody is non-null and inputOrInitBody’s source is null, then:
        if (inputOrInitBody is not null && inputOrInitBody.Source is null)
        {
            // If initBody is non-null and init["duplex"] does not exist, then throw a TypeError.
            if (initBody is not null && init.Get("duplex").IsUndefined())
            {
                TypeErrorHelper.Throw(
                    Engine,
                    "The 'duplex' property must be specified when providing a body.",
                    "constructor",
                    nameof(Request)
                );
            }

            // If this’s request’s mode is neither "same-origin" nor "cors", then throw a TypeError.
            if (
                requestInstance.RequestConcept.Mode != Mode.SameOrigin
                && requestInstance.RequestConcept.Mode != Mode.CORS
            )
            {
                TypeErrorHelper.Throw(
                    Engine,
                    "Request body with a non-null source is only allowed in same-origin or cors mode.",
                    "constructor",
                    nameof(Request)
                );
            }

            // Set this’s request’s use-CORS-preflight flag.
        }

        // Let finalBody be inputOrInitBody.
        var finalBody = inputOrInitBody;

        // If initBody is null and inputBody is non-null, then:
        if (initBody is null && inputBody is not null)
        {
            // If inputBody is unusable, then throw a TypeError.
            if (inputBody.Stream.Locked || inputBody.Stream.Disturbed)
            {
                TypeErrorHelper.Throw(
                    Engine,
                    "Input body has already been used.",
                    "constructor",
                    nameof(Request)
                );
            }

            // Set finalBody to the result of creating a proxy for inputBody.
            finalBody = inputBody;
        }

        // Set this’s request’s body to finalBody.
        requestInstance.RequestConcept.Body = finalBody;

        return requestInstance;
    }

    private static string NormalizeMethod(string method)
    {
        return method.ToUpperInvariant();
    }

    private static bool IsCorsSafelistedMethod([NotNullWhen(true)] string? method)
    {
        return !string.IsNullOrEmpty(method)
            && (
                method.Equals("GET", StringComparison.OrdinalIgnoreCase)
                || method.Equals("HEAD", StringComparison.OrdinalIgnoreCase)
                || method.Equals("POST", StringComparison.OrdinalIgnoreCase)
            );
    }

    private static bool IsValidMethod(string? method)
    {
        if (string.IsNullOrWhiteSpace(method))
        {
            return false;
        }

        foreach (var c in method)
        {
            if (char.IsControl(c) || char.IsWhiteSpace(c))
            {
                return false;
            }
        }

        return true;
    }
}
