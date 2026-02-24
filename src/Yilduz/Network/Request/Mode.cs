namespace Yilduz.Network.Request;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/Request/mode
/// </summary>
public static class Mode
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/mode#same-origin
    /// </summary>
    public static readonly string CORS = "cors";

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/mode#no-cors
    /// </summary>
    public static readonly string NoCors = "no-cors";

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/mode#same-origin
    /// </summary>
    public static readonly string SameOrigin = "same-origin";

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Request/mode#navigate
    /// </summary>
    public static readonly string Navigate = "navigate";
}
