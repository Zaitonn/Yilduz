using System.Collections.Generic;
using System.Linq;
using Yilduz.Network.Body;
using Yilduz.Network.Headers;
using Yilduz.URLs.URL;

namespace Yilduz.Network.Response;

internal sealed class ResponseConcept
{
    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-type
    /// </summary>
    public string Type { get; set; } = ResponseType.Default;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-aborted
    /// </summary>
    public bool AbortedFlag { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-url
    /// <br/>
    /// A pointer to the last URL in response's URL list, or null if the URL list is empty.
    /// </summary>
    public URLInstance? Url => URLList.LastOrDefault();

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-url-list
    /// </summary>
    public List<URLInstance> URLList { get; init; } = [];

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-status
    /// </summary>
    public ushort Status { get; set; } = 200;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-status-message
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-header-list
    /// </summary>
    public HeaderList HeaderList { get; init; } = [];

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-body
    /// </summary>
    public BodyConcept? Body { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-cache-state
    /// <br/>
    /// The empty string, "local", or "validated". Unless stated otherwise, it is the empty string.
    /// </summary>
    public string CacheState { get; set; } = string.Empty;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-cors-exposed-header-name-list
    /// </summary>
    public List<string> CORSExposedHeaderNameList { get; init; } = [];

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-range-requested-flag
    /// </summary>
    public bool RangeRequestedFlag { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-request-includes-credentials
    /// </summary>
    public bool RequestIncludesCredentials { get; set; } = true;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-timing-allow-passed
    /// </summary>
    public bool TimingAllowPassedFlag { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-body-info
    /// </summary>
    public ResponseBodyInfo BodyInfo { get; set; } = new();

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-service-worker-timing-info
    /// </summary>
    public object? ServiceWorkerTimingInfo { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-redirect-taint
    /// <br/>
    /// "same-origin", "same-site", or "cross-site". Initially "same-origin".
    /// </summary>
    public RedirectTaint RedirectTaint { get; set; } = RedirectTaint.SameOrigin;

    /// <summary>
    /// For a filtered response, this points to the internal (unfiltered) response.
    /// https://fetch.spec.whatwg.org/#concept-internal-response
    /// </summary>
    public ResponseConcept? InternalResponse { get; init; }

    /// <summary>
    /// Whether this response is a filtered response (i.e. wraps an internal response).
    /// https://fetch.spec.whatwg.org/#concept-filtered-response
    /// </summary>
    public bool IsFilteredResponse => InternalResponse is not null;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-network-error
    /// <br/>
    /// A network error is a response whose type is "error", status is 0, status message is the
    /// empty byte sequence, header list is « », body is null, and body info is a new response body info.
    /// </summary>
    public static ResponseConcept CreateNetworkError() =>
        new()
        {
            Type = ResponseType.Error,
            Status = 0,
            StatusMessage = string.Empty,
            Body = null,
            BodyInfo = new(),
        };

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-aborted-network-error
    /// <br/>
    /// An aborted network error is a network error whose aborted flag is set.
    /// </summary>
    public static ResponseConcept CreateAbortedNetworkError() =>
        new()
        {
            Type = ResponseType.Error,
            Status = 0,
            StatusMessage = string.Empty,
            Body = null,
            BodyInfo = new(),
            AbortedFlag = true,
        };

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-filtered-response-basic
    /// <br/>
    /// A basic filtered response whose type is "basic" and header list excludes any headers whose
    /// name is a forbidden response-header name.
    /// </summary>
    public ResponseConcept ToBasicFilteredResponse() =>
        new()
        {
            Type = ResponseType.Basic,
            HeaderList = [.. HeaderList.Where(h => !HttpHelper.IsForbiddenResponseHeader(h.Name))],
            InternalResponse = this,
        };

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-filtered-response-cors
    /// <br/>
    /// A CORS filtered response whose type is "cors" and header list excludes any headers whose
    /// name is not a CORS-safelisted response-header name given internal response's
    /// CORS-exposed header-name list.
    /// </summary>
    public ResponseConcept ToCORSFilteredResponse() =>
        new()
        {
            Type = ResponseType.Cors,
            HeaderList =
            [
                .. HeaderList.Where(h =>
                    IsCORSSafelistedResponseHeaderName(h.Name, CORSExposedHeaderNameList)
                ),
            ],
            InternalResponse = this,
        };

    /// <summary>
    /// https://fetch.spec.whatwg.org/#cors-safelisted-response-header-name
    /// <br/>
    /// A CORS-safelisted response-header name is a header name that is a byte-case-insensitive
    /// match for one of the CORS-safelisted names, or a name in the given CORS-exposed header-name list.
    /// </summary>
    private static bool IsCORSSafelistedResponseHeaderName(
        string name,
        IEnumerable<string> corsExposedHeaderNameList
    )
    {
        string[] safelisted =
        [
            "Cache-Control",
            "Content-Language",
            "Content-Length",
            "Content-Type",
            "Expires",
            "Last-Modified",
            "Pragma",
        ];
        foreach (var safe in safelisted)
        {
            if (name.Equals(safe, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        foreach (var exposed in corsExposedHeaderNameList)
        {
            if (name.Equals(exposed, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-filtered-response-opaque
    /// <br/>
    /// An opaque filtered response whose type is "opaque", URL list is « », status is 0,
    /// status message is the empty byte sequence, header list is « », body is null,
    /// and body info is a new response body info.
    /// </summary>
    public ResponseConcept ToOpaqueFilteredResponse() =>
        new()
        {
            Type = ResponseType.Opaque,
            URLList = [],
            Status = 0,
            StatusMessage = string.Empty,
            Body = null,
            BodyInfo = new(),
            InternalResponse = this,
        };

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-filtered-response-opaque-redirect
    /// <br/>
    /// An opaque-redirect filtered response whose type is "opaqueredirect", status is 0,
    /// status message is the empty byte sequence, header list is « », body is null,
    /// and body info is a new response body info.
    /// </summary>
    public ResponseConcept ToOpaqueRedirectFilteredResponse() =>
        new()
        {
            Type = ResponseType.OpaqueRedirect,
            URLList = [.. URLList],
            Status = 0,
            StatusMessage = string.Empty,
            Body = null,
            BodyInfo = new(),
            InternalResponse = this,
        };

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-clone
    /// </summary>
    public ResponseConcept Clone()
    {
        // If response is a filtered response, return a new identical filtered response whose
        // internal response is a clone of response's internal response.
        if (IsFilteredResponse)
        {
            return new()
            {
                Type = Type,
                AbortedFlag = AbortedFlag,
                Status = Status,
                StatusMessage = StatusMessage,
                HeaderList = [.. HeaderList],
                Body = Body,
                CacheState = CacheState,
                CORSExposedHeaderNameList = [.. CORSExposedHeaderNameList],
                RangeRequestedFlag = RangeRequestedFlag,
                RequestIncludesCredentials = RequestIncludesCredentials,
                TimingAllowPassedFlag = TimingAllowPassedFlag,
                BodyInfo = BodyInfo,
                ServiceWorkerTimingInfo = ServiceWorkerTimingInfo,
                RedirectTaint = RedirectTaint,
                URLList = [.. URLList],
                InternalResponse = InternalResponse!.Clone(),
            };
        }

        // Let newResponse be a copy of response, except for its body.
        // If response's body is non-null, set newResponse's body to the result of cloning response's body.
        var bodyClone = Body?.Clone();

        return new()
        {
            Type = Type,
            AbortedFlag = AbortedFlag,
            Status = Status,
            StatusMessage = StatusMessage,
            HeaderList = [.. HeaderList],
            Body = bodyClone,
            CacheState = CacheState,
            CORSExposedHeaderNameList = [.. CORSExposedHeaderNameList],
            RangeRequestedFlag = RangeRequestedFlag,
            RequestIncludesCredentials = RequestIncludesCredentials,
            TimingAllowPassedFlag = TimingAllowPassedFlag,
            BodyInfo = BodyInfo,
            ServiceWorkerTimingInfo = ServiceWorkerTimingInfo,
            RedirectTaint = RedirectTaint,
            URLList = [.. URLList],
        };
    }
}
