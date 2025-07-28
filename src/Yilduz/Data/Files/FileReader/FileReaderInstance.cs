using System;
using System.Text;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Yilduz.Data.Files.Blob;
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
        : base(engine) { }

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

    private void EnsureStateNotReading()
    {
        if (_isReading)
        {
            throw new InvalidOperationException("The FileReader is already reading a Blob or File");
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readAsArrayBuffer
    /// </summary>
    public void ReadAsArrayBuffer(JsValue blob)
    {
        EnsureBlob(blob, nameof(ReadAsArrayBuffer).ToJsStyleName(), out var blobInstance);
        EnsureStateNotReading();
        var total = (ulong)blobInstance.Value.Count;

        _isReading = true;
        ReadyState = FileReaderState.LOADING;
        Result = Undefined;
        Error = Null;

        Task.Run(() =>
        {
            try
            {
                DispatchEvent("loadstart");

                Result = blobInstance.ArrayBuffer();
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
        EnsureBlob(blob, nameof(ReadAsArrayBuffer).ToJsStyleName(), out var blobInstance);
        EnsureStateNotReading();
        _currentTotal = (ulong)blobInstance.Value.Count;

        _isReading = true;
        ReadyState = FileReaderState.LOADING;
        Result = Undefined;
        Error = Null;

        Task.Run(() =>
        {
            try
            {
                DispatchEvent("loadstart");

                Encoding textEncoding;
                try
                {
                    textEncoding = Encoding.GetEncoding(encoding);
                }
                catch
                {
                    textEncoding = Encoding.UTF8;
                }

                var data = blobInstance.Value.ToArray();
                var text = textEncoding.GetString(data);

                Result = text;
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
        EnsureBlob(blob, nameof(ReadAsArrayBuffer).ToJsStyleName(), out var blobInstance);
        EnsureStateNotReading();
        _currentTotal = (ulong)blobInstance.Value.Count;

        _isReading = true;
        ReadyState = FileReaderState.LOADING;
        Result = Undefined;
        Error = Null;

        Task.Run(() =>
        {
            try
            {
                DispatchEvent("loadstart");

                var base64 = Convert.ToBase64String([.. blobInstance.Value]);
                var mimeType = blobInstance.Type;

                if (string.IsNullOrEmpty(mimeType))
                {
                    mimeType = "application/octet-stream";
                }

                Result = $"data:{mimeType};base64,{base64}";
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

        try
        {
            DispatchEvent("abort");
        }
        catch { }
    }

    private void SetError(Exception ex)
    {
        Error = Engine.Intrinsics.Error.Construct(
            "An error occurred while reading the Blob or File: " + ex.Message
        );
        ReadyState = FileReaderState.DONE;

        try
        {
            DispatchEvent("error");
        }
        catch { }
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
