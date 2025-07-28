using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Data.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Data.Files.Blob;

public class BlobInstance : ObjectInstance
{
    private static readonly Encoding Utf8Encoding = new UTF8Encoding(false, true);
    private readonly BlobConstructor _blobConstructor;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/type
    /// </summary>
    public new string Type { get; protected internal set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/size
    /// </summary>
    public int Size => Value.Count;

    protected internal List<byte> Value { get; set; } = [];

    internal BlobInstance(
        Engine engine,
        BlobConstructor blobConstructor,
        JsValue blobParts,
        JsValue options
    )
        : base(engine)
    {
        _blobConstructor = blobConstructor;

        var optionsObject = !options.IsUndefined() ? options.AsObject() : null;

        Type = (optionsObject?.Get("type").ToString() ?? string.Empty).ToLowerInvariant().Trim();

        var endings = optionsObject?.Get("endings");
        var endingsStr =
            endings is null || endings.IsUndefined() == true ? "transparent" : endings.ToString();

        if (endingsStr != "transparent" && endingsStr != "native")
        {
            throw new JavaScriptException(
                engine.Intrinsics.TypeError,
                $"Failed to read the 'endings' property from 'BlobPropertyBag': The provided value '{endings}' is not a valid enum value of type EndingType."
            );
        }

        if (!blobParts.IsUndefined())
        {
            ProcessBlobParts(blobParts.AsObject(), endingsStr);
        }
    }

    private void ProcessBlobParts(ObjectInstance blobParts, string endings)
    {
        foreach (var part in blobParts)
        {
            if (part is BlobInstance blob)
            {
                Value.AddRange(blob.Value);
            }
            else if (part.IsArrayBuffer())
            {
                Value.AddRange(part.AsArrayBuffer()!);
            }
            else if (part.IsDataView())
            {
                Value.AddRange(part.AsDataView()!);
            }
            else if (
                part.IsBigInt64Array()
                || part.IsBigUint64Array()
                || part.IsFloat16Array()
                || part.IsFloat32Array()
                || part.IsFloat64Array()
                || part.IsInt8Array()
                || part.IsInt16Array()
                || part.IsInt32Array()
                || part.IsUint8Array()
                || part.IsUint16Array()
                || part.IsUint32Array()
                || part.IsUint8ClampedArray()
            )
            {
                Value.AddRange(part.Get("buffer").AsArrayBuffer()!);
            }
            else
            {
                var result = part.IsArray() ? string.Join(",", part.AsArray()) : part.ToString();

                if (endings == "native" && Environment.NewLine != "\n")
                {
                    result = result.Replace("\r\n", "\n").Replace("\n", "\r\n");
                }

                Value.AddRange(Utf8Encoding.GetBytes(result));
            }
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/size
    /// </summary>
    public string Text()
    {
        return Utf8Encoding.GetString([.. Value]);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/arrayBuffer
    /// </summary>
    public JsArrayBuffer ArrayBuffer()
    {
        return Engine.Intrinsics.ArrayBuffer.Construct([.. Value]);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/bytes
    /// </summary>
    public JsTypedArray Bytes()
    {
        return Engine.Intrinsics.Uint8Array.Construct(Value.ToArray());
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/stream
    /// </summary>
    public ReadableStreamInstance Stream()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/slice
    /// </summary>
    public BlobInstance Slice(int start = 0, int? end = null, string contentType = "")
    {
        if (start < 0)
        {
            start += Size;
        }

        if (end < 0)
        {
            end += Size;
        }
        if (end > Size)
        {
            end = Size;
        }

        end ??= Size;

        var slicedValue =
            start < 0 || start >= Size || end < 0 || start > end
                ? []
                : Value.Skip(start).Take((int)(end - start)).ToList();
        var blobInstance = (BlobInstance)_blobConstructor.Construct(Arguments.Empty, Undefined);

        blobInstance.Value = slicedValue;
        blobInstance.Type = contentType;

        return blobInstance;
    }
}
