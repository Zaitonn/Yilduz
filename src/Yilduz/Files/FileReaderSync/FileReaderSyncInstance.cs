using System;
using System.Text;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Files.Blob;
using Yilduz.Utils;
using SystemEncoding = System.Text.Encoding;

namespace Yilduz.Files.FileReaderSync;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/FileReaderSync
/// </summary>
public sealed class FileReaderSyncInstance : ObjectInstance
{
    internal FileReaderSyncInstance(Engine engine)
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
            nameof(FileReaderSync)
        );
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReaderSync/readAsArrayBuffer
    /// </summary>
    public JsValue ReadAsArrayBuffer(JsValue blob)
    {
        EnsureBlob(blob, FileReaderSyncPrototype.ReadAsArrayBufferName, out var blobInstance);

        return blobInstance.ArrayBuffer();
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReaderSync/readAsText
    /// </summary>
    public JsValue ReadAsText(JsValue blob, string encoding = "UTF-8")
    {
        EnsureBlob(blob, FileReaderSyncPrototype.ReadAsTextName, out var blobInstance);

        try
        {
            SystemEncoding textEncoding;
            try
            {
                textEncoding = SystemEncoding.GetEncoding(encoding);
            }
            catch
            {
                textEncoding = SystemEncoding.UTF8;
            }

            var data = blobInstance.Value.ToArray();
            var text = textEncoding.GetString(data);

            return text;
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
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReaderSync/readAsDataURL
    /// </summary>
    public JsValue ReadAsDataURL(JsValue blob)
    {
        EnsureBlob(blob, FileReaderSyncPrototype.ReadAsDataURLName, out var blobInstance);

        try
        {
            var base64 = Convert.ToBase64String([.. blobInstance.Value]);
            var mimeType = blobInstance.Type;

            if (string.IsNullOrEmpty(mimeType))
            {
                mimeType = "application/octet-stream";
            }

            return $"data:{mimeType};base64,{base64}";
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
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReaderSync/readAsBinaryString
    /// Note: This method is deprecated but still implemented for compatibility
    /// </summary>
    public JsValue ReadAsBinaryString(JsValue blob)
    {
        EnsureBlob(blob, FileReaderSyncPrototype.ReadAsBinaryStringName, out var blobInstance);

        try
        {
            var data = blobInstance.Value.ToArray();
            var binaryString = new StringBuilder(data.Length);

            foreach (var b in data)
            {
                binaryString.Append((char)b);
            }

            return binaryString.ToString();
        }
        catch (Exception ex)
        {
            throw new JavaScriptException(
                Engine.Intrinsics.Error,
                "An error occurred while reading the Blob or File: " + ex.Message
            );
        }
    }
}
