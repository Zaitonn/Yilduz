namespace Yilduz.Network.Request;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/Request/cache
/// </summary>
public static class Cache
{
    /// <summary>
    /// Fetch will inspect the HTTP cache on the way to the network. If the HTTP cache contains a matching fresh response it will be returned. If the HTTP cache contains a matching stale-while-revalidate response it will be returned, and a conditional network fetch will be made to update the entry in the HTTP cache. If the HTTP cache contains a matching stale response, a conditional network fetch will be returned to update the entry in the HTTP cache. Otherwise, a non-conditional network fetch will be returned to update the entry in the HTTP cache.
    /// </summary>
    public static readonly string Default = "default";

    /// <summary>
    /// Fetch behaves as if there is no HTTP cache at all.
    /// </summary>
    public static readonly string NoStore = "no-store";

    /// <summary>
    /// Fetch behaves as if there is no HTTP cache on the way to the network. Ergo, it creates a normal request and updates the HTTP cache with the response.
    /// </summary>
    public static readonly string Reload = "reload";

    /// <summary>
    /// Fetch creates a conditional request if there is a response in the HTTP cache and a normal request otherwise. It then updates the HTTP cache with the response.
    /// </summary>
    public static readonly string NoCache = "no-cache";

    /// <summary>
    /// Fetch uses any response in the HTTP cache matching the request, not paying attention to staleness. If there was no response, it creates a normal request and updates the HTTP cache with the response.
    /// </summary>
    public static readonly string ForceCache = "force-cache";

    /// <summary>
    /// Fetch uses any response in the HTTP cache matching the request, not paying attention to staleness. If there was no response, it returns a network error. (Can only be used when request’s mode is "same-origin". Any cached redirects will be followed assuming request’s redirect mode is "follow" and the redirects do not violate request’s mode.)
    /// </summary>
    public static readonly string OnlyIfCached = "only-if-cached";
}
