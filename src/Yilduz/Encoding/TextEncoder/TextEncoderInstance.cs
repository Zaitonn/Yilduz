using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using SystemEncoding = System.Text.Encoding;

namespace Yilduz.Encoding.TextEncoder;

public sealed class TextEncoderInstance : ObjectInstance
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/TextEncoder/encoding
    /// </summary>
    public string Encoding { get; } = "utf-8";

    internal TextEncoderInstance(Engine engine)
        : base(engine) { }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/TextEncoder/encode
    /// </summary>
    public JsTypedArray Encode(string input)
    {
        var utf8Bytes = SystemEncoding.UTF8.GetBytes(input);

        return Engine.Intrinsics.Uint8Array.Construct(utf8Bytes);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/TextEncoder/encodeInto
    /// </summary>
    public ObjectInstance EncodeInto(string input, JsTypedArray destination)
    {
        var destinationArray = destination.AsObject();
        var byteLength = (int)destinationArray.Get("byteLength").AsNumber();

        var utf8Bytes = SystemEncoding.UTF8.GetBytes(input);
        var written = Math.Min(utf8Bytes.Length, byteLength);

        for (int i = 0; i < written; i++)
        {
            destinationArray[i] = (int)utf8Bytes[i];
        }

        var read = input.Length;

        if (written < utf8Bytes.Length)
        {
            var tempBytes = new byte[written];
            Array.Copy(utf8Bytes, tempBytes, written);

            try
            {
                var decodedString = SystemEncoding.UTF8.GetString(tempBytes);
                read = decodedString.Length;
            }
            catch
            {
                var validBytes = written;
                while (validBytes > 0 && !IsValidUtf8Sequence(utf8Bytes, validBytes))
                {
                    validBytes--;
                }

                if (validBytes > 0)
                {
                    var validTempBytes = new byte[validBytes];
                    Array.Copy(utf8Bytes, validTempBytes, validBytes);
                    var decodedString = SystemEncoding.UTF8.GetString(validTempBytes);
                    read = decodedString.Length;
                    written = validBytes;
                }
                else
                {
                    read = 0;
                    written = 0;
                }
            }
        }

        var result = Engine.Intrinsics.Object.Construct(Arguments.Empty);
        result.Set("read", read);
        result.Set("written", written);

        return result;
    }

    private static bool IsValidUtf8Sequence(byte[] bytes, int length)
    {
        try
        {
            var tempBytes = new byte[length];
            Array.Copy(bytes, tempBytes, length);
            SystemEncoding.UTF8.GetString(tempBytes);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
