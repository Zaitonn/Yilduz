using System;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Data.Files.Blob;
using Yilduz.Data.Files.FileReaderSync;
using Yilduz.Events.EventTarget;
using Yilduz.Events.ProgressEvent;
using Yilduz.Utils;

namespace Yilduz.Data.Files.FileReader;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/FileReader
/// </summary>
public sealed class FileReaderInstance : EventTargetInstance
{
    private WebApiIntrinsics? _webApiIntrinsics;
    private bool _isReading;
    private ulong? _currentTotal;
    private readonly FileReaderSyncInstance _fileReaderSyncInstance;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readyState
    /// </summary>
    public FileReaderState ReadyState { get; private set; } = FileReaderState.EMPTY;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/result
    /// </summary>
    public JsValue Result { get; private set; } = Undefined;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/error
    /// </summary>
    public JsValue Error { get; private set; } = Null;

    public JsValue OnLoadStart { get; internal set; } = Undefined;
    public JsValue OnLoad { get; internal set; } = Undefined;
    public JsValue OnLoadEnd { get; internal set; } = Undefined;
    public JsValue OnError { get; internal set; } = Undefined;
    public JsValue OnAbort { get; internal set; } = Undefined;
    public JsValue OnProgress { get; internal set; } = Undefined;

    internal FileReaderInstance(Engine engine)
        : base(engine)
    {
        _fileReaderSyncInstance = new(Engine);
    }

    private void EnsureBlob(JsValue blob, string methodName, out BlobInstance blobInstance)
    {
        blobInstance = null!;

        if (blob is BlobInstance b)
        {
            blobInstance = b;
            return;
        }

        TypeErrorHelper.Throw(
            Engine,
            "parameter 1 is not of type 'Blob'.",
            methodName,
            nameof(FileReader)
        );
    }

    private void PrepareForReading(JsValue blob, string methodName)
    {
        if (_isReading)
        {
            throw new InvalidOperationException(
                "FileReader is already reading a Blob or File. You cannot start a new read operation until the current one is complete."
            );
        }

        if (blob is not BlobInstance blobInstance)
        {
            TypeErrorHelper.Throw(
                Engine,
                "parameter 1 is not of type 'Blob'.",
                methodName,
                nameof(FileReader)
            );
            return;
        }

        _currentTotal = (ulong)blobInstance.Value.Count;
        _isReading = true;
        ReadyState = FileReaderState.LOADING;
        Result = Undefined;
        Error = Null;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readAsArrayBuffer
    /// </summary>
    public void ReadAsArrayBuffer(JsValue blob)
    {
        PrepareForReading(blob, FileReaderPrototype.ReadAsArrayBufferName);

        Task.Run(() =>
        {
            try
            {
                DispatchEvent("loadstart");

                Result = _fileReaderSyncInstance.ReadAsArrayBuffer(blob);
                ReadyState = FileReaderState.DONE;

                DispatchEvent("progress");
                DispatchEvent("load");
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
            finally
            {
                _isReading = false;
                DispatchEvent("loadend");
            }
        });
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readAsText
    /// </summary>
    public void ReadAsText(JsValue blob, string encoding = "UTF-8")
    {
        PrepareForReading(blob, FileReaderPrototype.ReadAsTextName);

        Task.Run(() =>
        {
            try
            {
                DispatchEvent("loadstart");

                Result = _fileReaderSyncInstance.ReadAsText(blob, encoding);
                ReadyState = FileReaderState.DONE;

                DispatchEvent("progress");
                DispatchEvent("load");
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
            finally
            {
                _isReading = false;
                DispatchEvent("loadend");
            }
        });
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readAsDataURL
    /// </summary>
    public void ReadAsDataURL(JsValue blob)
    {
        PrepareForReading(blob, FileReaderPrototype.ReadAsDataURLName);

        Task.Run(() =>
        {
            try
            {
                DispatchEvent("loadstart");

                Result = _fileReaderSyncInstance.ReadAsDataURL(blob);
                ReadyState = FileReaderState.DONE;

                DispatchEvent("progress");
                DispatchEvent("load");
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
            finally
            {
                _isReading = false;
                DispatchEvent("loadend");
            }
        });
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReaderSync/readAsBinaryString
    /// Note: This method is deprecated but still implemented for compatibility
    /// </summary>
    public JsValue ReadAsBinaryString(JsValue blob)
    {
        EnsureBlob(blob, FileReaderPrototype.ReadAsBinaryStringName, out var blobInstance);

        try
        {
            return _fileReaderSyncInstance.ReadAsBinaryString(blobInstance);
        }
        catch (Exception ex)
        {
            throw new JavaScriptException(
                Engine.Intrinsics.Error,
                "An error occurred while reading the Blob or File: " + ex.Message
            );
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/abort
    /// </summary>
    public void Abort()
    {
        if (ReadyState != FileReaderState.LOADING)
        {
            return;
        }

        ReadyState = FileReaderState.DONE;
        Result = Undefined;
        _isReading = false;

        DispatchEvent("abort");
    }

    private void SetError(Exception ex)
    {
        Error = ex is JavaScriptException javaScriptException
            ? javaScriptException.Error
            : Engine.Intrinsics.Error.Construct(
                "An error occurred while reading the Blob or File: " + ex.Message
            );

        ReadyState = FileReaderState.DONE;
        DispatchEvent("error");
    }

    private void DispatchEvent(string eventType)
    {
        _webApiIntrinsics ??= Engine.GetWebApiIntrinsics();
        try
        {
            var progressEvent = (ProgressEventInstance)
                _webApiIntrinsics.ProgressEvent.Construct([eventType], Undefined);

            progressEvent.LengthComputable = true;

            if (_currentTotal.HasValue)
            {
                progressEvent.Total = _currentTotal.Value;
                progressEvent.Loaded = ReadyState == FileReaderState.DONE ? _currentTotal.Value : 0;
            }

            DispatchEvent(progressEvent);
        }
        catch { }
    }
}
