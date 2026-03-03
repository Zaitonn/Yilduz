using Jint;
using Jint.Native;
using Yilduz.Models;

namespace Yilduz.Data.FileReaderSync;

internal sealed class FileReaderSyncPrototype : PrototypeBase<FileReaderSyncInstance>
{
    public FileReaderSyncPrototype(Engine engine, FileReaderSyncConstructor constructor)
        : base(engine, nameof(FileReaderSync), constructor)
    {
        RegisterMethod("readAsArrayBuffer", ReadAsArrayBuffer, 1);
        RegisterMethod("readAsText", ReadAsText, 1);
        RegisterMethod("readAsDataURL", ReadAsDataURL, 1);
        RegisterMethod("readAsBinaryString", ReadAsBinaryString, 1);
    }

    private static JsValue ReadAsArrayBuffer(FileReaderSyncInstance reader, JsValue[] arguments)
    {
        return reader.ReadAsArrayBuffer(arguments[0]);
    }

    private static JsValue ReadAsText(FileReaderSyncInstance reader, JsValue[] arguments)
    {
        var encoding = arguments.Length > 1 ? arguments[1].ToString() : "UTF-8";
        return reader.ReadAsText(arguments[0], encoding);
    }

    private static JsValue ReadAsDataURL(FileReaderSyncInstance reader, JsValue[] arguments)
    {
        return reader.ReadAsDataURL(arguments[0]);
    }

    private static JsValue ReadAsBinaryString(FileReaderSyncInstance reader, JsValue[] arguments)
    {
        return reader.ReadAsBinaryString(arguments[0]);
    }
}
