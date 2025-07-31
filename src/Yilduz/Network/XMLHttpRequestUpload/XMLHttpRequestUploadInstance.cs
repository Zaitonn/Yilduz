using Jint;
using Yilduz.Network.XMLHttpRequestEventTarget;

namespace Yilduz.Network.XMLHttpRequestUpload;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequestUpload
/// </summary>
public sealed class XMLHttpRequestUploadInstance : XMLHttpRequestEventTargetInstance
{
    internal XMLHttpRequestUploadInstance(Engine engine)
        : base(engine) { }
}
