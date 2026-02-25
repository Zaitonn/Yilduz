namespace Yilduz.Network.Fetch;

/// <summary>
/// https://fetch.spec.whatwg.org/#deferred-fetch-record-invoke-state
/// </summary>
internal enum InvokeState
{
    Pending,

    Sent,

    Aborted,
}
