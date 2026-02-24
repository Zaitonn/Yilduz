using System;
using Jint;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Extensions;
using Yilduz.Network.Body;
using Yilduz.Network.Headers;
using Yilduz.Utils;

namespace Yilduz.Network.Request;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/Request
/// </summary>
public sealed class RequestInstance : BodyInstance
{
    internal RequestConcept RequestConcept { get; }

    internal RequestInstance(
        Engine engine,
        WebApiIntrinsics webApiIntrinsics,
        RequestConcept request
    )
        : base(engine, webApiIntrinsics)
    {
        RequestConcept = request;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/method
    /// </summary>
    public string Method => RequestConcept.Method;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/url
    /// </summary>
    public string Url => RequestConcept.Url?.ToString() ?? string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/headers
    /// </summary>
    public required HeadersInstance Headers { get; init; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/destination
    /// </summary>
    public string Destination => RequestConcept.Destination;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/referrer
    /// </summary>
    public string Referrer
    {
        get
        {
            var str = RequestConcept.Referrer.ToString();
            return str switch
            {
                "no-referrer" => string.Empty,
                "client" => "about:client",
                _ => str,
            };
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/referrerPolicy
    /// </summary>
    public string ReferrerPolicy => RequestConcept.ReferrerPolicy;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/mode
    /// </summary>
    public string Mode => RequestConcept.Mode;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/credentials
    /// </summary>
    public string Credentials => RequestConcept.CredentialsMode;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/cache
    /// </summary>
    public string Cache => RequestConcept.CacheMode;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/redirect
    /// </summary>
    public string Redirect => RequestConcept.RedirectMode;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/integrity
    /// </summary>
    public string Integrity => RequestConcept.IntegrityMetadata;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/keepalive
    /// </summary>
    public bool Keepalive => RequestConcept.Keepalive;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/isReloadNavigation
    /// </summary>
    public bool IsReloadNavigation => RequestConcept.ReloadNavigationFlag;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/isHistoryNavigation
    /// </summary>
    public bool IsHistoryNavigation => RequestConcept.HistoryNavigationFlag;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/signal
    /// </summary>
    public required AbortSignalInstance Signal { get; init; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/duplex
    /// </summary>
    public string Duplex => "half";

    internal override BodyConcept? BodyConcept => RequestConcept.Body;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/clone
    /// </summary>
    public RequestInstance Clone()
    {
        // If this is unusable, then throw a TypeError.
        if (BodyUsed)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Body has already been used.",
                nameof(Clone).ToJsStyleName(),
                "Request"
            );
        }

        // Let clonedRequest be the result of cloning this’s request.
        var clonedConcept = RequestConcept.Clone();

        // Assert: this’s signal is non-null.
        if (Signal is null)
        {
            throw new InvalidOperationException("Request signal cannot be null for cloning.");
        }

        // Let clonedSignal be the result of creating a dependent abort signal from « this’s signal », using AbortSignal and this’s relevant realm.
        var clonedSignal = _webApiIntrinsics.AbortSignal.CreateDependentSignal(Signal);

        // Let clonedRequestObject be the result of creating a Request object, given clonedRequest, this’s headers’s guard, clonedSignal and this’s relevant realm.
        var clonedRequestObject = new RequestInstance(Engine, _webApiIntrinsics, clonedConcept)
        {
            Prototype = _webApiIntrinsics.Request.PrototypeObject,
            Headers = _webApiIntrinsics.Headers.Construct(clonedConcept.HeaderList, Headers.Guard),
            Signal = clonedSignal,
        };

        return clonedRequestObject;
    }
}
