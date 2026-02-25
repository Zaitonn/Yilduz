using Jint.Native.Function;
using Yilduz.Network.Request;

namespace Yilduz.Network.Fetch;

/// <summary>
/// https://fetch.spec.whatwg.org/#deferred-fetch-record
/// </summary>
internal record DeferredFetchRecord
{
    /// <summary>
    /// https://fetch.spec.whatwg.org/#deferred-fetch-record-request
    /// </summary>
    public RequestConcept? Request { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#deferred-fetch-record-notify-invoked
    /// </summary>
    public Function? NotifyInvoked { get; set; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#deferred-fetch-record-invoke-state
    /// </summary>
    public InvokeState InvokeState { get; set; } = InvokeState.Pending;
}
