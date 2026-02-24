namespace Yilduz.Network.Response;

/// <summary>
/// https://fetch.spec.whatwg.org/#concept-response-redirect-taint
/// </summary>
internal enum RedirectTaint
{
    SameOrigin,

    SameSite,

    CrossSite,
}
