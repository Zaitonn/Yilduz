using System.Collections.Generic;

namespace Yilduz.Network.Fetch;

/// <summary>
/// https://fetch.spec.whatwg.org/#fetch-timing-info
/// </summary>
internal struct FetchTimingInfo
{
    public FetchTimingInfo()
    {
        ServerTimingHeaders = [];
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-start-time
    /// </summary>
    public double StartTime { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-redirect-start-time
    /// </summary>
    public double RedirectStartTime { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-redirect-end-time
    /// </summary>
    public double RedirectEndTime { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-post-redirect-start-time
    /// </summary>
    public double PostRedirectStartTime { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-final-service-worker-start-time
    /// </summary>
    public double FinalServiceWorkerStartTime { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-final-network-request-start-time
    /// </summary>
    public double FinalNetworkRequestStartTime { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-first-interim-network-response-start-time
    /// </summary>
    public double FirstInterimNetworkResponseStartTime { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-final-network-response-start-time
    /// </summary>
    public double FinalNetworkResponseStartTime { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-end-time
    /// </summary>
    public double EndTime { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-final-connection-timing-info
    /// </summary>
    public object? FinalConnectionTimingInfo { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-service-worker-timing-info
    /// </summary>
    public object? ServiceWorkerTimingInfo { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-server-timing-headers
    /// </summary>
    public List<string> ServerTimingHeaders { get; init; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#fetch-timing-info-render-blocking
    /// </summary>
    public bool RenderBlocking { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#create-an-opaque-timing-info
    /// </summary>
    public static FetchTimingInfo CreateOpaqueTimingInfo(FetchTimingInfo timingInfo) =>
        new()
        {
            StartTime = timingInfo.StartTime,
            PostRedirectStartTime = timingInfo.PostRedirectStartTime,
        };
}
