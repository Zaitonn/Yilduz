namespace Yilduz.Network.Request;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/Request/credentials
/// </summary>
public static class Credentials
{
    /// <summary>
    /// Never send credentials in the request or include credentials in the response.
    /// </summary>
    public static readonly string Omit = "omit";

    /// <summary>
    /// Only send and include credentials for same-origin requests. This is the default.
    /// </summary>
    public static readonly string SameOrigin = "same-origin";

    /// <summary>
    /// Always include credentials, even for cross-origin requests.
    /// </summary>
    public static readonly string Include = "include";
}
