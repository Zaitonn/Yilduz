namespace Yilduz.Network.Headers;

internal enum Guard
{
    None,

    Request,

    RequestNoCors,

    Response,

    Immutable,
}
