using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Utils;

namespace Yilduz.Data.Files.FileReader;

internal sealed class FileReaderPrototype : ObjectInstance
{
    private static readonly string ReadyStateName = nameof(FileReaderInstance.ReadyState)
        .ToJsStyleName();
    private static readonly string ReadyStateGetterName = ReadyStateName.ToJsGetterName();
    private static readonly string ResultName = nameof(FileReaderInstance.Result).ToJsStyleName();
    private static readonly string ResultGetterName = ResultName.ToJsGetterName();
    private static readonly string ErrorName = nameof(FileReaderInstance.Error).ToJsStyleName();
    private static readonly string ErrorGetterName = ErrorName.ToJsGetterName();

    private static readonly string ReadAsArrayBufferName = nameof(ReadAsArrayBuffer)
        .ToJsStyleName();
    private static readonly string ReadAsTextName = nameof(ReadAsText).ToJsStyleName();
    private static readonly string ReadAsDataURLName = nameof(ReadAsDataURL).ToJsStyleName();
    private static readonly string AbortName = nameof(Abort).ToJsStyleName();

    private static readonly string OnLoadStartName = nameof(FileReaderInstance.OnLoadStart)
        .ToLowerInvariant();
    private static readonly string OnLoadName = nameof(FileReaderInstance.OnLoad)
        .ToLowerInvariant();
    private static readonly string OnLoadEndName = nameof(FileReaderInstance.OnLoadEnd)
        .ToLowerInvariant();
    private static readonly string OnErrorName = nameof(FileReaderInstance.OnError)
        .ToLowerInvariant();
    private static readonly string OnAbortName = nameof(FileReaderInstance.OnAbort)
        .ToLowerInvariant();
    private static readonly string OnProgressName = nameof(FileReaderInstance.OnProgress)
        .ToLowerInvariant();

    public FileReaderPrototype(Engine engine, FileReaderConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(FileReader));
        SetOwnProperty("constructor", new(constructor, false, false, false));

        FastSetProperty(
            ReadyStateName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ReadyStateGetterName, GetReadyState),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            ResultName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ResultGetterName, GetResult),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            ErrorName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ErrorGetterName, GetError),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            nameof(FileReaderState.EMPTY),
            new((int)FileReaderState.EMPTY, false, false, true)
        );
        FastSetProperty(
            nameof(FileReaderState.LOADING),
            new((int)FileReaderState.LOADING, false, false, true)
        );
        FastSetProperty(
            nameof(FileReaderState.DONE),
            new((int)FileReaderState.DONE, false, false, true)
        );

        FastSetProperty(
            ReadAsArrayBufferName,
            new(
                new ClrFunction(Engine, ReadAsArrayBufferName, ReadAsArrayBuffer),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            ReadAsTextName,
            new(new ClrFunction(Engine, ReadAsTextName, ReadAsText), false, false, true)
        );
        FastSetProperty(
            ReadAsDataURLName,
            new(new ClrFunction(Engine, ReadAsDataURLName, ReadAsDataURL), false, false, true)
        );
        FastSetProperty(
            AbortName,
            new(new ClrFunction(Engine, AbortName, Abort), false, false, true)
        );

        // Event handler properties
        FastSetProperty(
            OnLoadStartName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnLoadStartName.ToJsGetterName(), GetOnLoadStart),
                set: new ClrFunction(engine, OnLoadStartName.ToJsSetterName(), SetOnLoadStart),
                false,
                true
            )
        );
        FastSetProperty(
            OnLoadName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnLoadName.ToJsGetterName(), GetOnLoad),
                set: new ClrFunction(engine, OnLoadName.ToJsSetterName(), SetOnLoad),
                false,
                true
            )
        );
        FastSetProperty(
            OnLoadEndName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnLoadEndName.ToJsGetterName(), GetOnLoadEnd),
                set: new ClrFunction(engine, OnLoadEndName.ToJsSetterName(), SetOnLoadEnd),
                false,
                true
            )
        );
        FastSetProperty(
            OnErrorName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnErrorName.ToJsGetterName(), GetOnError),
                set: new ClrFunction(engine, OnErrorName.ToJsSetterName(), SetOnError),
                false,
                true
            )
        );
        FastSetProperty(
            OnAbortName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnAbortName.ToJsGetterName(), GetOnAbort),
                set: new ClrFunction(engine, OnAbortName.ToJsSetterName(), SetOnAbort),
                false,
                true
            )
        );
        FastSetProperty(
            OnProgressName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnProgressName.ToJsGetterName(), GetOnProgress),
                set: new ClrFunction(engine, OnProgressName.ToJsSetterName(), SetOnProgress),
                false,
                true
            )
        );
    }

    private JsValue GetReadyState(JsValue thisObject, JsValue[] arguments)
    {
        return (int)thisObject.EnsureThisObject<FileReaderInstance>().ReadyState;
    }

    private JsValue GetResult(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<FileReaderInstance>().Result;
    }

    private JsValue GetError(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<FileReaderInstance>().Error;
    }

    private JsValue ReadAsArrayBuffer(JsValue thisObject, JsValue[] arguments)
    {
        var reader = thisObject.EnsureThisObject<FileReaderInstance>();
        arguments.EnsureCount(Engine, 1, ReadAsArrayBufferName, nameof(FileReader));

        reader.ReadAsArrayBuffer(arguments.At(0));

        return Undefined;
    }

    private JsValue ReadAsText(JsValue thisObject, JsValue[] arguments)
    {
        var reader = thisObject.EnsureThisObject<FileReaderInstance>();

        arguments.EnsureCount(Engine, 1, ReadAsTextName, nameof(FileReader));

        var encoding = arguments.Length > 1 ? arguments[1].ToString() : "UTF-8";
        reader.ReadAsText(arguments.At(0), encoding);
        return Undefined;
    }

    private JsValue ReadAsDataURL(JsValue thisObject, JsValue[] arguments)
    {
        var reader = thisObject.EnsureThisObject<FileReaderInstance>();
        arguments.EnsureCount(Engine, 1, ReadAsDataURLName, nameof(FileReader));
        reader.ReadAsDataURL(arguments.At(0));

        return Undefined;
    }

    private JsValue Abort(JsValue thisObject, JsValue[] arguments)
    {
        thisObject.EnsureThisObject<FileReaderInstance>().Abort();
        return Undefined;
    }

    // Event handler property getters and setters
    private JsValue GetOnLoadStart(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<FileReaderInstance>().OnLoadStart;
    }

    private JsValue SetOnLoadStart(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<FileReaderInstance>();
        instance.OnLoadStart = arguments.At(0);
        return instance.OnLoadStart;
    }

    private JsValue GetOnLoad(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<FileReaderInstance>().OnLoad;
    }

    private JsValue SetOnLoad(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<FileReaderInstance>();
        instance.OnLoad = arguments.At(0);
        return instance.OnLoad;
    }

    private JsValue GetOnLoadEnd(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<FileReaderInstance>().OnLoadEnd;
    }

    private JsValue SetOnLoadEnd(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<FileReaderInstance>();
        instance.OnLoadEnd = arguments.At(0);
        return instance.OnLoadEnd;
    }

    private JsValue GetOnError(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<FileReaderInstance>().OnError;
    }

    private JsValue SetOnError(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<FileReaderInstance>();
        instance.OnError = arguments.At(0);
        return instance.OnError;
    }

    private JsValue GetOnAbort(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<FileReaderInstance>().OnAbort;
    }

    private JsValue SetOnAbort(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<FileReaderInstance>();
        instance.OnAbort = arguments.At(0);
        return instance.OnAbort;
    }

    private JsValue GetOnProgress(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<FileReaderInstance>().OnProgress;
    }

    private JsValue SetOnProgress(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<FileReaderInstance>();
        instance.OnProgress = arguments.At(0);
        return instance.OnProgress;
    }
}
