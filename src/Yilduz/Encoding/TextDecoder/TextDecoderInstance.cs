using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
    private Decoder? _decoder;
    private bool _bomSeen;

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

    private bool _doNotFlush;

    internal TextDecoderInstance(Engine engine, JsValue label, JsValue options)
        : base(engine)
    {
        var encodingLabel = label.IsUndefined() ? "utf-8" : label.ToString();

        var fatal = false;
        var ignoreBOM = false;

        if (options.IsObject())
        {
            var optionsObj = options.AsObject();

            fatal = optionsObj.Get("fatal").ConvertToBoolean();
            ignoreBOM = optionsObj.Get("ignoreBOM").ConvertToBoolean();
        }

        Fatal = fatal;
        IgnoreBOM = ignoreBOM;

        try
        {
            Encoding = EncodingHelper.NormalizeEncodingName(encodingLabel);
            _encoding = SystemEncoding.GetEncoding(
                Encoding,
                EncoderFallback.ExceptionFallback,
                fatal ? DecoderFallback.ExceptionFallback : DecoderFallback.ReplacementFallback
            );

            ResetDecoderState();
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

        if (!_doNotFlush)
        {
            ResetDecoderState();
        }

        _doNotFlush = stream;

        var bytes = input.IsUndefined() ? Array.Empty<byte>() : input.TryAsBytes();

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

        var copiedBytes = new byte[bytes.Length];
        if (bytes.Length > 0)
        {
            Array.Copy(bytes, copiedBytes, bytes.Length);
        }

        var flush = !_doNotFlush;
        return DecodeBytes(copiedBytes, flush);
    }

    private string DecodeBytes(byte[] bytes, bool flush)
    {
        try
        {
            var charCount = _decoder!.GetCharCount(bytes, 0, bytes.Length, flush);

            if (charCount == 0)
            {
                return string.Empty;
            }

            var chars = new char[charCount];
            var written = _decoder.GetChars(bytes, 0, bytes.Length, chars, 0, flush);

            return SerializeOutput(chars, written);
        }
        catch (DecoderFallbackException ex) when (Fatal)
        {
            TypeErrorHelper.Throw(
                Engine,
                $"Failed to decode: {ex.Message}",
                "decode",
                nameof(TextDecoder)
            );
            return null!;
        }
    }

    private string SerializeOutput(char[] chars, int length)
    {
        if (length == 0)
        {
            return string.Empty;
        }

        var span = new ReadOnlySpan<char>(chars, 0, length);
        var startIndex = 0;

        if (!_bomSeen && span.Length > 0)
        {
            if (span[0] == '\uFEFF')
            {
                if (!IgnoreBOM)
                {
                    startIndex = 1;
                }
            }

            _bomSeen = true;
        }

        if (startIndex >= span.Length)
        {
            return string.Empty;
        }

#if NETSTANDARD
        return new(span[startIndex..].ToArray());
#else
        return new(span[startIndex..]);
#endif
    }

    [MemberNotNull(nameof(_decoder))]
    private void ResetDecoderState()
    {
        _decoder = _encoding.GetDecoder();
        _decoder.Fallback = _encoding.DecoderFallback;
        _bomSeen = false;
    }
}
