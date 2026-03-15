using Jint;
using Yilduz.Models;

namespace Yilduz.Network.XMLHttpRequestUpload;

internal sealed class XMLHttpRequestUploadPrototype(
    Engine engine,
    XMLHttpRequestUploadConstructor constructor
) : PrototypeBase<XMLHttpRequestUploadInstance>(engine, nameof(XMLHttpRequestUpload), constructor);
