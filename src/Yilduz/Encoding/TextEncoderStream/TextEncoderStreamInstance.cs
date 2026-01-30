using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Encoding.TextEncoder;
using Yilduz.Extensions;
using Yilduz.Streams.ReadableStream;
using Yilduz.Streams.TransformStream;
using Yilduz.Streams.TransformStreamDefaultController;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Encoding.TextEncoderStream;

/// <summary>
/// https://encoding.spec.whatwg.org/#interface-textencoderstream
/// </summary>
public sealed class TextEncoderStreamInstance : ObjectInstance, IGenericTransformStream
{
    private readonly TextEncoderInstance _encoder;
    private readonly TransformStreamInstance _transformStream;

    /// <summary>
    /// https://encoding.spec.whatwg.org/#dom-textencoderstream-encoding
    /// </summary>
    public string Encoding => _encoder.Encoding;

    /// <summary>
    /// The ReadableStream instance controlled by this object
    /// </summary>
    public ReadableStreamInstance Readable => _transformStream.Readable;

    /// <summary>
    /// The WritableStream instance controlled by this object
    /// </summary>
    public WritableStreamInstance Writable => _transformStream.Writable;

    internal TextEncoderStreamInstance(Engine engine)
        : base(engine)
    {
        var webApiIntrinsics = Engine.GetWebApiIntrinsics();
        _encoder = (TextEncoderInstance)
            webApiIntrinsics.TextEncoder.Construct(Arguments.Empty, Undefined);

        var transformer = new JsObject(engine);
        transformer.Set(
            "transform",
            new ClrFunction(engine, "transform", (_, args) => Transform(args.At(0), args.At(1)))
        );

        _transformStream = webApiIntrinsics.TransformStream.Construct(
            transformer,
            Undefined,
            Undefined
        );
    }

    private JsValue Transform(JsValue input, JsValue controller)
    {
        if (controller is TransformStreamDefaultControllerInstance tsdc)
        {
            var encoded = _encoder.Encode(input.ToArgumentString());
            tsdc.Enqueue(encoded);
        }
        else
        {
            TypeErrorHelper.Throw(Engine, "controller is not TransformStreamDefaultController");
        }

        return Undefined;
    }
}
