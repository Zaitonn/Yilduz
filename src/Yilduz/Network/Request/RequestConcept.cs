using System.Collections.Generic;
using System.Linq;
using Jint.Native;
using Yilduz.Network.Body;
using Yilduz.Network.Headers;
using Yilduz.URLs.URL;

namespace Yilduz.Network.Request;

internal sealed class RequestConcept
{
    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-method
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-url
    /// <br/>
    /// Implementations are encouraged to make this a pointer to the first URL in request’s URL list. It is provided as a distinct field solely for the convenience of other standards hooking into Fetch.
    /// </summary>
    public URLInstance Url
    {
        get => URLList.First();
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#local-urls-only-flag
    /// </summary>
    public bool LocalURLsOnlyFlag { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-header-list
    /// </summary>
    public HeaderList HeaderList { get; init; } = [];

    /// <summary>
    /// https://fetch.spec.whatwg.org/#unsafe-request-flag
    /// </summary>
    public bool UnsafeRequestFlag { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-body
    /// </summary>
    public BodyConcept? Body { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-reserved-client
    /// </summary>
    public JsValue Client { get; set; } = JsValue.Null;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-reserved-client
    /// </summary>
    public JsValue ReservedClient { get; set; } = JsValue.Null;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-replaces-client-id
    /// </summary>
    public string ReplacesClientID { get; set; } = string.Empty;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-traversable-for-user-promptsF
    /// </summary>
    public string TraversableForUserPrompts { get; set; } = "client";

    /// <summary>
    /// https://fetch.spec.whatwg.org/#request-keepalive-flag
    /// </summary>
    public bool Keepalive { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#request-initiator-type
    /// </summary>
    public InitiatorType? InitiatorType { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#request-service-workers-mode
    /// </summary>
    public ServiceWorkersMode ServiceWorkersMode { get; set; } = ServiceWorkersMode.All;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-initiator
    /// </summary>
    public string Initiator { get; set; } = string.Empty;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-destination
    /// </summary>
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-priority
    /// </summary>
    public Priority Priority { get; set; } = Priority.Auto;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#request-internal-priority
    /// </summary>
    public JsValue InternalPriority { get; set; } = JsValue.Null;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-origin
    /// </summary>
    public string Origin { get; set; } = "client";

    /// <summary>
    /// https://fetch.spec.whatwg.org/#request-top-level-navigation-initiator-origin
    /// </summary>
    public object? TopLevelNavigationInitiatorOrigin { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#request-policy-container
    /// </summary>
    public object? PolicyContainer { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-referrer
    /// </summary>
    public JsValue Referrer { get; set; } = "client";

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-referrer-policy
    /// </summary>
    public string ReferrerPolicy { get; set; } = string.Empty;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-mode
    /// </summary>
    public string Mode { get; set; } = Request.Mode.NoCors;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#use-cors-preflight-flag
    /// </summary>
    public bool UseCORSPreflightFlag { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-credentials-mode
    /// </summary>
    public string CredentialsMode { get; set; } = Credentials.SameOrigin;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-use-url-credentials-flag
    /// </summary>
    public bool UseURLCredentialsFlag { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-cache-mode
    /// </summary>
    public string CacheMode { get; set; } = Cache.Default;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-redirect-mode
    /// </summary>
    public string RedirectMode { get; set; } = Redirect.Follow;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-integrity-metadata
    /// </summary>
    public string IntegrityMetadata { get; set; } = string.Empty;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-nonce-metadata
    /// </summary>
    public string CryptographicNonceMetadata { get; set; } = string.Empty;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-parser-metadata
    /// </summary>
    public string ParserMetadata { get; set; } = string.Empty;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-reload-navigation-flag
    /// </summary>
    public bool ReloadNavigationFlag { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-history-navigation-flag
    /// </summary>
    public bool HistoryNavigationFlag { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#request-user-activation
    /// </summary>
    public bool UserActivation { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#request-render-blocking
    /// </summary>
    public bool RenderBlocking { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-url-list
    /// </summary>
    public List<URLInstance> URLList { get; init; } = [];

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-current-url
    /// </summary>
    public URLInstance CurrentURL => URLList.Last();

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-redirect-count
    /// </summary>
    public uint RedirectCount { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-response-tainting
    /// </summary>
    public ResponseTainting ResponseTainting { get; set; } = ResponseTainting.Basic;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-prevent-no-cache-cache-control-header-modification-flag
    /// </summary>
    public bool PreventNoCacheCacheControlHeaderModificationFlag { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-request-done-flag
    /// </summary>
    public bool DoneFlag { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#timing-allow-failed
    /// </summary>
    public bool TimingAllowFailedFlag { get; set; }

    public RequestConcept Clone()
    {
        var bodyClone = Body?.Clone();

        return new()
        {
            // Url = Url,
            Method = Method,
            HeaderList = [.. HeaderList],
            Destination = Destination,
            Origin = Origin,
            Body = bodyClone,
            Referrer = Referrer,
            ReferrerPolicy = ReferrerPolicy,
            Mode = Mode,
            CredentialsMode = CredentialsMode,
            CacheMode = CacheMode,
            RedirectMode = RedirectMode,
            IntegrityMetadata = IntegrityMetadata,
            Keepalive = Keepalive,
            ReloadNavigationFlag = ReloadNavigationFlag,
            HistoryNavigationFlag = HistoryNavigationFlag,
            UnsafeRequestFlag = UnsafeRequestFlag,
            URLList = [.. URLList],
            UserActivation = UserActivation,
            RenderBlocking = RenderBlocking,
            Initiator = Initiator,
            Client = Client,
            PreventNoCacheCacheControlHeaderModificationFlag =
                PreventNoCacheCacheControlHeaderModificationFlag,
            TopLevelNavigationInitiatorOrigin = TopLevelNavigationInitiatorOrigin,
            ServiceWorkersMode = ServiceWorkersMode,
            TraversableForUserPrompts = TraversableForUserPrompts,
            UseCORSPreflightFlag = UseCORSPreflightFlag,
            UseURLCredentialsFlag = UseURLCredentialsFlag,
            InternalPriority = InternalPriority,
            InitiatorType = InitiatorType,
            PolicyContainer = PolicyContainer,
            TimingAllowFailedFlag = TimingAllowFailedFlag,
            CryptographicNonceMetadata = CryptographicNonceMetadata,
            ParserMetadata = ParserMetadata,
            RedirectCount = RedirectCount,
            ResponseTainting = ResponseTainting,
            ReplacesClientID = ReplacesClientID,
            ReservedClient = ReservedClient,
            LocalURLsOnlyFlag = LocalURLsOnlyFlag,
            Priority = Priority,
            DoneFlag = DoneFlag,
        };
    }
}
