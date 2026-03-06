using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;

namespace Yilduz.Data.FileReader;

internal sealed class FileReaderPrototype : PrototypeBase<FileReaderInstance>
{
    public FileReaderPrototype(Engine engine, FileReaderConstructor constructor)
        : base(engine, nameof(FileReader), constructor)
    {
        RegisterProperty("readyState", reader => (int)reader.ReadyState);
        RegisterProperty("result", reader => reader.Result);
        RegisterProperty("error", reader => reader.Error);

        RegisterConstant("EMPTY", FileReaderReadyState.EMPTY);
        RegisterConstant("LOADING", FileReaderReadyState.LOADING);
        RegisterConstant("DONE", FileReaderReadyState.DONE);

        RegisterMethod("readAsArrayBuffer", ReadAsArrayBuffer, 1);
        RegisterMethod("readAsText", ReadAsText, 1);
        RegisterMethod("readAsDataURL", ReadAsDataURL, 1);
        RegisterMethod("readAsBinaryString", ReadAsBinaryString, 1);
        RegisterMethod("abort", Abort);

        RegisterProperty("onloadstart", GetOnLoadStart, SetOnLoadStart, Types.Object);
        RegisterProperty("onload", GetOnLoad, SetOnLoad, Types.Object);
        RegisterProperty("onloadend", GetOnLoadEnd, SetOnLoadEnd, Types.Object);
        RegisterProperty("onerror", GetOnError, SetOnError, Types.Object);
        RegisterProperty("onabort", GetOnAbort, SetOnAbort, Types.Object);
        RegisterProperty("onprogress", GetOnProgress, SetOnProgress, Types.Object);
    }

    private static JsValue ReadAsArrayBuffer(FileReaderInstance reader, JsValue[] arguments)
    {
        reader.ReadAsArrayBuffer(arguments.At(0));
        return Undefined;
    }

    private static JsValue ReadAsText(FileReaderInstance reader, JsValue[] arguments)
    {
        var encoding = arguments.Length > 1 ? arguments[1].ToString() : "UTF-8";
        reader.ReadAsText(arguments.At(0), encoding);
        return Undefined;
    }

    private static JsValue ReadAsDataURL(FileReaderInstance reader, JsValue[] arguments)
    {
        reader.ReadAsDataURL(arguments.At(0));
        return Undefined;
    }

    private static JsValue ReadAsBinaryString(FileReaderInstance reader, JsValue[] arguments)
    {
        return reader.ReadAsBinaryString(arguments[0]);
    }

    private static JsValue Abort(FileReaderInstance reader, JsValue[] arguments)
    {
        reader.Abort();
        return Undefined;
    }

    private static JsValue GetOnLoadStart(FileReaderInstance reader)
    {
        return reader.OnLoadStart;
    }

    private static JsValue SetOnLoadStart(FileReaderInstance reader, JsValue value)
    {
        reader.OnLoadStart = value;
        return reader.OnLoadStart;
    }

    private static JsValue GetOnLoad(FileReaderInstance reader)
    {
        return reader.OnLoad;
    }

    private static JsValue SetOnLoad(FileReaderInstance reader, JsValue value)
    {
        reader.OnLoad = value;
        return reader.OnLoad;
    }

    private static JsValue GetOnLoadEnd(FileReaderInstance reader)
    {
        return reader.OnLoadEnd;
    }

    private static JsValue SetOnLoadEnd(FileReaderInstance reader, JsValue value)
    {
        reader.OnLoadEnd = value;
        return reader.OnLoadEnd;
    }

    private static JsValue GetOnError(FileReaderInstance reader)
    {
        return reader.OnError;
    }

    private static JsValue SetOnError(FileReaderInstance reader, JsValue value)
    {
        reader.OnError = value;
        return reader.OnError;
    }

    private static JsValue GetOnAbort(FileReaderInstance reader)
    {
        return reader.OnAbort;
    }

    private static JsValue SetOnAbort(FileReaderInstance reader, JsValue value)
    {
        reader.OnAbort = value;
        return reader.OnAbort;
    }

    private static JsValue GetOnProgress(FileReaderInstance reader)
    {
        return reader.OnProgress;
    }

    private static JsValue SetOnProgress(FileReaderInstance reader, JsValue value)
    {
        reader.OnProgress = value;
        return reader.OnProgress;
    }
}
