using Jint;
using Yilduz.Network.Body;
using Yilduz.Network.Headers;
using Yilduz.Utils;

namespace Yilduz.Network.Response;

/// <summary>
/// https://developer.mozilla.org/zh-CN/docs/Web/API/Response
/// </summary>
public sealed class ResponseInstance : BodyInstance
{
    internal ResponseConcept ResponseConcept { get; }

    internal ResponseInstance(
        Engine engine,
        WebApiIntrinsics webApiIntrinsics,
        ResponseConcept response
    )
        : base(engine, webApiIntrinsics)
    {
        ResponseConcept = response;
    }

    /// <summary>
    /// https://developer.mozilla.org/zh-CN/docs/Web/API/Response/type
    /// </summary>
    public new string Type => ResponseConcept.Type;

    /// <summary>
    /// https://developer.mozilla.org/zh-CN/docs/Web/API/Response/url
    /// </summary>
    public string Url
    {
        get
        {
            var url = ResponseConcept.Url;
            if (url is null)
                return string.Empty;
            // Serialize with exclude fragment = true
            var serialized = url.Href;
            var hashIndex = serialized.IndexOf('#');
            return hashIndex >= 0 ? serialized[..hashIndex] : serialized;
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/zh-CN/docs/Web/API/Response/redirected
    /// </summary>
    public bool Redirected => ResponseConcept.URLList.Count > 1;

    /// <summary>
    /// https://developer.mozilla.org/zh-CN/docs/Web/API/Response/status
    /// </summary>
    public int Status => ResponseConcept.Status;

    /// <summary>
    /// https://developer.mozilla.org/zh-CN/docs/Web/API/Response/ok
    /// </summary>
    public bool Ok => ResponseConcept.Status is >= 200 and <= 299;

    /// <summary>
    /// https://developer.mozilla.org/zh-CN/docs/Web/API/Response/statusText
    /// </summary>
    public string StatusText => ResponseConcept.StatusMessage;

    /// <summary>
    /// https://developer.mozilla.org/zh-CN/docs/Web/API/Response/headers
    /// </summary>
    public required HeadersInstance Headers { get; init; }

    internal override BodyConcept? BodyConcept => ResponseConcept.Body;

    /// <summary>
    /// https://developer.mozilla.org/zh-CN/docs/Web/API/Response/clone
    /// </summary>
    public ResponseInstance Clone()
    {
        // If this is unusable, then throw a TypeError.
        if (BodyUsed)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Body has already been used.",
                ResponsePrototype.CloneName,
                "Response"
            );
        }

        // Let clonedResponse be the result of cloning this's response.
        var clonedConcept = ResponseConcept.Clone();

        // Return the result of creating a Response object, given clonedResponse,
        // this's headers's guard, and this's relevant realm.
        return _webApiIntrinsics.Response.Create(clonedConcept, Headers.Guard);
    }
}
