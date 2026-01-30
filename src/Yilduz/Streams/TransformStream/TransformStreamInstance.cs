using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Promise;
using Jint.Runtime;
using Yilduz.Streams.ReadableStream;
using Yilduz.Streams.TransformStreamDefaultController;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.TransformStream;

/// <summary>
/// TransformStream implementation according to WHATWG Streams Standard
/// https://streams.spec.whatwg.org/#transform-streams
/// </summary>
public sealed partial class TransformStreamInstance : ObjectInstance, IGenericTransformStream
{
    /// <summary>
    /// The ReadableStream instance controlled by this object
    /// <br/>
    /// https://streams.spec.whatwg.org/#ts-readable
    /// </summary>
    public ReadableStreamInstance Readable => _readable;

    /// <summary>
    /// The WritableStream instance controlled by this object
    /// <br/>
    /// https://streams.spec.whatwg.org/#ts-writable
    /// </summary>
    public WritableStreamInstance Writable => _writable;

    /// <summary>
    /// Whether there was backpressure on [[readable]] the last time it was observed
    /// <br/>
    /// https://streams.spec.whatwg.org/#transformstream-backpressure
    /// </summary>
    internal bool? Backpressure { get; set; }

    /// <summary>
    /// A promise which is fulfilled and replaced every time the value of [[backpressure]] changes
    /// <br/>
    /// https://streams.spec.whatwg.org/#transformstream-backpressurechangepromise
    /// </summary>
    internal ManualPromise? BackpressureChangePromise { get; set; }

    /// <summary>
    /// A TransformStreamDefaultController created with the ability to control [[readable]] and [[writable]]
    /// <br/>
    /// https://streams.spec.whatwg.org/#transformstream-controller
    /// </summary>
    internal TransformStreamDefaultControllerInstance? Controller { get; set; }

    /// <summary>
    /// A boolean flag set to true when the stream is transferred
    /// https://streams.spec.whatwg.org/#transformstream-detached
    /// </summary>
    internal bool Detached { get; set; }

    private readonly ReadableStreamInstance _readable;
    private readonly WritableStreamInstance _writable;

    /// <summary>
    /// Constructor for user-created TransformStream instances
    /// https://streams.spec.whatwg.org/#ts-constructor
    /// </summary>
    public TransformStreamInstance(
        Engine engine,
        JsValue transformer,
        JsValue writableStrategy,
        JsValue readableStrategy
    )
        : base(engine)
    {
        // If transformer is missing, set it to null.
        transformer = transformer.IsUndefined() ? Null : transformer;

        // Let transformerDict be transformer, converted to an IDL value of type Transformer.
        var transformerDict = transformer.IsObject() ? transformer.AsObject() : null;

        // If transformerDict["readableType"] exists, throw a RangeError exception.
        if (transformerDict?.Get("readableType") is { } readableType && !readableType.IsUndefined())
        {
            throw new JavaScriptException(
                ErrorHelper.Create(
                    Engine,
                    "RangeError",
                    "TransformStream constructor does not support a readableType property"
                )
            );
        }

        // If transformerDict["writableType"] exists, throw a RangeError exception.
        if (transformerDict?.Get("writableType") is { } writableType && !writableType.IsUndefined())
        {
            throw new JavaScriptException(
                ErrorHelper.Create(
                    Engine,
                    "RangeError",
                    "TransformStream constructor does not support a writableType property"
                )
            );
        }

        // Let readableHighWaterMark be ? ExtractHighWaterMark(readableStrategy, 0).
        var readableHighWaterMark = AbstractOperations.ExtractHighWaterMark(
            engine,
            readableStrategy,
            0
        );

        // Let readableSizeAlgorithm be ! ExtractSizeAlgorithm(readableStrategy).
        var readableSizeAlgorithm = AbstractOperations.ExtractSizeAlgorithm(
            engine,
            readableStrategy
        );

        // Let writableHighWaterMark be ? ExtractHighWaterMark(writableStrategy, 1).
        var writableHighWaterMark = AbstractOperations.ExtractHighWaterMark(
            engine,
            writableStrategy,
            1
        );

        // Let writableSizeAlgorithm be ! ExtractSizeAlgorithm(writableStrategy).
        var writableSizeAlgorithm = AbstractOperations.ExtractSizeAlgorithm(
            engine,
            writableStrategy
        );

        // Let startPromise be a new promise.
        var startPromise = Engine.Advanced.RegisterPromise();

        // Perform ! InitializeTransformStream(this, startPromise, writableHighWaterMark, writableSizeAlgorithm, readableHighWaterMark, readableSizeAlgorithm).
        InitializeTransformStream(
            startPromise,
            writableHighWaterMark,
            writableSizeAlgorithm,
            readableHighWaterMark,
            readableSizeAlgorithm,
            out _readable,
            out _writable
        );

        // Perform ? SetUpTransformStreamDefaultControllerFromTransformer(this, transformer, transformerDict).
        SetUpDefaultControllerFromTransformer(transformer, transformerDict);

        // If transformerDict["start"] exists,
        //   then resolve startPromise with the result of invoking transformerDict["start"] with argument list « this.[[controller]] » and callback this value transformer.
        if (transformerDict?.Get("start") is { } startMethod && !startMethod.IsUndefined())
        {
            try
            {
                var startResult = startMethod
                    .AsFunctionInstance()
                    .Call(thisObj: transformer, [Controller ?? Undefined]);

                startPromise.Resolve(startResult);
            }
            catch (JavaScriptException e)
            {
                startPromise.Reject(e.Error);
            }
        }
        else
        {
            // Otherwise, resolve startPromise with undefined.
            startPromise.Resolve(Undefined);
        }
    }
}
