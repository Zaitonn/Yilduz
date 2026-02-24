namespace Yilduz.Network.Response;

/// <summary>
/// https://fetch.spec.whatwg.org/#concept-response-type
/// </summary>
internal static class ResponseType
{
    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-responsetype-basic
    /// </summary>
    public static readonly string Basic = "basic";

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-responsetype-cors
    /// </summary>
    public static readonly string Cors = "cors";

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-responsetype-default
    /// </summary>
    public static readonly string Default = "default";

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-responsetype-error
    /// </summary>
    public static readonly string Error = "error";

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-responsetype-opaque
    /// </summary>
    public static readonly string Opaque = "opaque";

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-responsetype-opaqueredirect
    /// </summary>
    public static readonly string OpaqueRedirect = "opaqueredirect";
}
