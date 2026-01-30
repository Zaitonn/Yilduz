using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Encoding.TextDecoder;
using Yilduz.Streams.ReadableStream;
using Yilduz.Streams.TransformStream;
using Yilduz.Streams.TransformStreamDefaultController;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Encoding.TextDecoderStream;

/// <summary>
/// https://encoding.spec.whatwg.org/#interface-textdecoderstream
/// </summary>
public sealed class TextDecoderStreamInstance : ObjectInstance, IGenericTransformStream
{
    private readonly TextDecoderInstance _decoder;
    private readonly TransformStreamInstance _transformStream;
    private readonly ObjectInstance _streamTrueOptions;
    private readonly ObjectInstance _streamFalseOptions;

    /// <summary>
    /// https://encoding.spec.whatwg.org/#dom-textdecoderstream-encoding
    /// </summary>
    public string Encoding => _decoder.Encoding;

    /// <summary>
    /// https://encoding.spec.whatwg.org/#dom-textdecoderstream-fatal
    /// </summary>
    public bool Fatal => _decoder.Fatal;

    /// <summary>
    /// https://encoding.spec.whatwg.org/#dom-textdecoderstream-ignorebom
    /// </summary>
    public bool IgnoreBOM => _decoder.IgnoreBOM;

    /// <summary>
    /// The ReadableStream instance controlled by this object
    /// </summary>
    public ReadableStreamInstance Readable => _transformStream.Readable;

    /// <summary>
    /// The WritableStream instance controlled by this object
    /// </summary>
    public WritableStreamInstance Writable => _transformStream.Writable;

    internal TextDecoderStreamInstance(Engine engine, JsValue label, JsValue options)
        : base(engine)
    {
        var webApiIntrinsics = Engine.GetWebApiIntrinsics();
        _decoder = (TextDecoderInstance)
            webApiIntrinsics.TextDecoder.Construct([label, options], Undefined);

        _streamTrueOptions = engine.Intrinsics.Object.Construct(Arguments.Empty);
        _streamTrueOptions.Set("stream", true);
        _streamFalseOptions = engine.Intrinsics.Object.Construct(Arguments.Empty);
        _streamFalseOptions.Set("stream", false);

        var transformer = new JsObject(engine);
        transformer.Set(
            "transform",
            new ClrFunction(engine, "transform", (_, args) => Transform(args.At(0), args.At(1)))
        );
        transformer.Set("flush", new ClrFunction(engine, "flush", (_, args) => Flush(args.At(0))));

        _transformStream = webApiIntrinsics.TransformStream.Construct(
            transformer,
            Undefined,
            Undefined
        );
    }

    private JsValue Transform(JsValue chunk, JsValue controller)
    {
        var output = _decoder.Decode(chunk, _streamTrueOptions);

        if (controller is TransformStreamDefaultControllerInstance tsdc)
        {
            if (!string.IsNullOrEmpty(output))
            {
                tsdc.Enqueue(output);
            }
        }
        else
        {
            TypeErrorHelper.Throw(Engine, "controller is not TransformStreamDefaultController");
        }

        return Undefined;
    }

    private JsValue Flush(JsValue controller)
    {
        if (controller is TransformStreamDefaultControllerInstance tsdc)
        {
            var output = _decoder.Decode(Undefined, _streamFalseOptions);
            if (!string.IsNullOrEmpty(output))
            {
                tsdc.Enqueue(output);
            }
        }
        else
        {
            TypeErrorHelper.Throw(Engine, "controller is not TransformStreamDefaultController");
        }

        return Undefined;
    }
}
