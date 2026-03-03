using System;
using System.Threading;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Data.Blob;
using Yilduz.Data.FileReaderSync;
using Yilduz.Events.EventTarget;
using Yilduz.Events.ProgressEvent;
using Yilduz.Utils;

namespace Yilduz.Data.FileReader;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/FileReader
/// </summary>
public sealed class FileReaderInstance : EventTargetInstance
{
    private bool _isReading;
    private ulong? _currentTotal;
    private readonly FileReaderSyncInstance _fileReaderSyncInstance;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readyState
    /// </summary>
    public FileReaderReadyState ReadyState { get; private set; } = FileReaderReadyState.EMPTY;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/result
    /// </summary>
    public JsValue Result { get; private set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/error
    /// </summary>
    public JsValue Error { get; private set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/loadstart_event
    /// </summary>
    public JsValue OnLoadStart { get; internal set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/load_event
    /// </summary>
    public JsValue OnLoad { get; internal set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/loadend_event
    /// </summary>
    public JsValue OnLoadEnd { get; internal set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/error_event
    /// </summary>
    public JsValue OnError { get; internal set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/abort_event
    /// </summary>
    public JsValue OnAbort { get; internal set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/progress_event
    /// </summary>
    public JsValue OnProgress { get; internal set; } = Null;

    internal FileReaderInstance(Engine engine)
        : base(engine)
    {
        _fileReaderSyncInstance = new(Engine);
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
        ReadyState = FileReaderReadyState.LOADING;
        Result = Undefined;
        Error = Null;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readAsArrayBuffer
    /// </summary>
    public void ReadAsArrayBuffer(JsValue blob)
    {
        PrepareForReading(blob, "readAsArrayBuffer");

        _webApiIntrinsics.EventLoop.QueueMacrotask(() =>
        {
            try
            {
                DispatchEvent("loadstart");

                Result = _fileReaderSyncInstance.ReadAsArrayBuffer(blob);
                ReadyState = FileReaderReadyState.DONE;

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
        PrepareForReading(blob, "readAsText");

        _webApiIntrinsics.EventLoop.QueueMacrotask(() =>
        {
            try
            {
                DispatchEvent("loadstart");

                Result = _fileReaderSyncInstance.ReadAsText(blob, encoding);
                ReadyState = FileReaderReadyState.DONE;

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
        PrepareForReading(blob, "readAsDataURL");

        _webApiIntrinsics.EventLoop.QueueMacrotask(() =>
        {
            try
            {
                DispatchEvent("loadstart");

                Result = _fileReaderSyncInstance.ReadAsDataURL(blob);
                ReadyState = FileReaderReadyState.DONE;

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
        _fileReaderSyncInstance.EnsureBlob(blob, "readAsBinaryString", out var blobInstance);

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
        if (ReadyState != FileReaderReadyState.LOADING)
        {
            return;
        }

        ReadyState = FileReaderReadyState.DONE;
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

        ReadyState = FileReaderReadyState.DONE;
        DispatchEvent("error");
    }

    private void DispatchEvent(string eventType)
    {
        bool entered = false;
        try
        {
            Monitor.TryEnter(Engine, _webApiIntrinsics.Options.WaitingTimeout, ref entered);

            if (entered)
            {
                var progressEvent = (ProgressEventInstance)
                    _webApiIntrinsics.ProgressEvent.Construct([eventType], Undefined);

                progressEvent.LengthComputable = true;

                if (_currentTotal.HasValue)
                {
                    progressEvent.Total = _currentTotal.Value;
                    progressEvent.Loaded =
                        ReadyState == FileReaderReadyState.DONE ? _currentTotal.Value : 0;
                }

                DispatchEvent(progressEvent);
            }
        }
        finally
        {
            if (entered)
            {
                Monitor.Exit(Engine);
            }
        }
    }
}
