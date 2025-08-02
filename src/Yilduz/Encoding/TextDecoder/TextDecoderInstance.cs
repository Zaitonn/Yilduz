using System;
using System.Collections.Generic;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Extensions;
using Yilduz.Utils;
using SystemEncoding = System.Text.Encoding;

namespace Yilduz.Encoding.TextDecoder;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/TextDecoder
/// </summary>
public sealed class TextDecoderInstance : ObjectInstance
{
    private readonly SystemEncoding _encoding;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/TextDecoder/encoding
    /// </summary>
    public string Encoding { get; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/TextDecoder/fatal
    /// </summary>
    public bool Fatal { get; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/TextDecoder/ignoreBOM
    /// </summary>
    public bool IgnoreBOM { get; }

    internal TextDecoderInstance(Engine engine, JsValue label, JsValue options)
        : base(engine)
    {
        var encodingLabel = label.IsUndefined() ? "utf-8" : label.ToString();

        var fatal = false;
        var ignoreBOM = false;

        if (options.IsObject())
        {
            var optionsObj = options.AsObject();

            fatal = optionsObj.Get("fatal").ToBoolean();
            ignoreBOM = optionsObj.Get("ignoreBOM").ToBoolean();
        }

        Fatal = fatal;
        IgnoreBOM = ignoreBOM;

        try
        {
            Encoding = EncodingHelper.NormalizeEncodingName(encodingLabel);
            _encoding = SystemEncoding.GetEncoding(Encoding);
        }
        catch (ArgumentException e)
        {
            throw new JavaScriptException(
                ErrorHelper.Create(
                    Engine,
                    "RangeError",
                    $"Failed to create TextDecoder for encoding '{encodingLabel}': {e.Message}"
                )
            );
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/TextDecoder/decode
    /// </summary>
    public string Decode(JsValue input, JsValue options)
    {
        var stream = false;

        if (options.IsObject())
        {
            var optionsObj = options.AsObject();
            var streamValue = optionsObj.Get("stream");
            stream = !streamValue.IsUndefined() && streamValue.AsBoolean();
        }

        var bytes = input.IsUndefined() ? [] : input.TryAsBytes();

        if (bytes is null)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Input must be an ArrayBuffer, DataView, TypedArray, or Array of numbers.",
                "decode",
                nameof(TextDecoder)
            );
            return null!;
        }

        try
        {
            if (!IgnoreBOM && bytes.Length > 0)
            {
                bytes = RemoveBOMIfPresent(bytes);
            }

            var result = _encoding.GetString(bytes);
            return result;
        }
        catch (Exception ex) when (Fatal)
        {
            TypeErrorHelper.Throw(
                Engine,
                $"Failed to decode: {ex.Message}",
                "decode",
                nameof(TextDecoder)
            );
            return null!;
        }
        catch
        {
            var decoder = _encoding.GetDecoder();
            decoder.Fallback = System.Text.DecoderFallback.ReplacementFallback;

            var charCount = decoder.GetCharCount(bytes, 0, bytes.Length);
            var chars = new char[charCount];
            decoder.GetChars(bytes, 0, bytes.Length, chars, 0);

            return new string(chars);
        }
    }

    private byte[] RemoveBOMIfPresent(byte[] bytes)
    {
        if (
            _encoding == SystemEncoding.UTF8
            && bytes.Length >= 3
            && bytes[0] == 0xEF
            && bytes[1] == 0xBB
            && bytes[2] == 0xBF
        )
        {
            return [.. bytes.Skip(3)];
        }

        if (
            _encoding == SystemEncoding.Unicode
            && bytes.Length >= 2
            && bytes[0] == 0xFF
            && bytes[1] == 0xFE
        )
        {
            return [.. bytes.Skip(2)];
        }

        if (
            _encoding == SystemEncoding.BigEndianUnicode
            && bytes.Length >= 2
            && bytes[0] == 0xFE
            && bytes[1] == 0xFF
        )
        {
            return [.. bytes.Skip(2)];
        }

        return bytes;
    }
}
