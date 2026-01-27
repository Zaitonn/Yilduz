using System;
using Jint;
using Jint.Native;

namespace Yilduz.Streams;

internal static class Miscellaneous
{
    /// <summary>
    /// https://streams.spec.whatwg.org/#can-transfer-array-buffer
    /// </summary>
    public static bool CanTransferArrayBuffer(JsValue o)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#is-non-negative-number
    /// </summary>
    public static bool IsNonNegativeNumber(JsValue value)
    {
        if (!value.IsNumber())
        {
            return false;
        }

        var number = value.AsNumber();
        if (double.IsNaN(number) || number < 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#transfer-array-buffer
    /// </summary>
    public static JsValue TransferArrayBuffer(JsValue o)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#abstract-opdef-cloneasuint8array
    /// </summary>
    public static JsValue CloneAsUint8Array(JsValue o)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#abstract-opdef-structuredclone
    /// </summary>
    public static JsValue StructuredClone(JsValue o)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#abstract-opdef-cancopydatablockbytes
    /// </summary>
    public static bool CanCopyDataBlockBytes(
        JsValue toBuffer,
        int toIndex,
        JsValue fromBuffer,
        int fromIndex,
        int count
    )
    {
        throw new NotImplementedException();
    }
}
