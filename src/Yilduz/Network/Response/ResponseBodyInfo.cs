namespace Yilduz.Network.Response;

/// <summary>
/// https://fetch.spec.whatwg.org/#response-body-info
/// </summary>
internal sealed class ResponseBodyInfo
{
    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-body-info-content-type
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-body-info-encoded-size
    /// </summary>
    public ulong EncodedSize { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-response-body-info-decoded-body-size
    /// </summary>
    public ulong DecodedBodySize { get; set; }
}
