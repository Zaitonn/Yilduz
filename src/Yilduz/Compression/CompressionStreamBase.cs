using System;
using System.IO;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Compression.Providers;
using Yilduz.Extensions;
using Yilduz.Streams.ReadableStream;
using Yilduz.Streams.TransformStream;
using Yilduz.Streams.TransformStreamDefaultController;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Compression;

public abstract class CompressionStreamBase : ObjectInstance, IGenericTransformStream
{
    private readonly ICompressionProvider _provider;
    private readonly TransformStreamInstance _transformStream;
    private readonly string _className;
    private bool _flushed;

    private protected CompressionStreamBase(
        Engine engine,
        WebApiIntrinsics webApiIntrinsics,
        ICompressionProvider provider,
        string className
    )
        : base(engine)
    {
        _provider = provider;
        _className = className;

        var transformer = new JsObject(engine);
        transformer.Set(
            "transform",
            new ClrFunction(
                engine,
                "transform",
                (_, args) => HandleTransform(args.At(0), args.At(1))
            )
        );
        transformer.Set(
            "flush",
            new ClrFunction(engine, "flush", (_, args) => HandleFlush(args.At(0)))
        );
        _transformStream = webApiIntrinsics.TransformStream.CreateInstance(
            transformer,
            Undefined,
            Undefined
        );

        webApiIntrinsics.Options.CancellationToken.Register(() => _provider.Dispose());
    }

    private JsValue HandleTransform(JsValue chunk, JsValue controller)
    {
        var tsdc = EnsureController(controller);

        var bytes = chunk.TryAsBytes(false);
        if (bytes is null)
        {
            TypeErrorHelper.Throw(
                Engine,
                "chunk must be an ArrayBuffer, DataView or TypedArray.",
                "transform",
                _className
            );
        }

        try
        {
            var result = _provider.Transform(bytes);
            EnqueueIfNotEmpty(tsdc, result);
        }
        catch (InvalidDataException ex)
        {
            TypeErrorHelper.Throw(Engine, "Invalid compressed data: " + ex.Message);
        }

        return Undefined;
    }

    private JsValue HandleFlush(JsValue controller)
    {
        if (_flushed)
        {
            return Undefined;
        }

        _flushed = true;

        var tsdc = EnsureController(controller);

        try
        {
            var result = _provider.Flush();
            EnqueueIfNotEmpty(tsdc, result);
        }
        catch (InvalidDataException ex)
        {
            TypeErrorHelper.Throw(Engine, "Invalid compressed data: " + ex.Message);
        }

        return Undefined;
    }

    private void EnqueueIfNotEmpty(TransformStreamDefaultControllerInstance tsdc, byte[] data)
    {
        if (data.Length > 0)
        {
            tsdc.Enqueue(Engine.Intrinsics.Uint8Array.Construct(data));
        }
    }

    private TransformStreamDefaultControllerInstance EnsureController(JsValue controller)
    {
        if (controller is TransformStreamDefaultControllerInstance tsdc)
        {
            return tsdc;
        }

        TypeErrorHelper.Throw(Engine, "controller is not TransformStreamDefaultController");
        return null;
    }

    internal static ICompressionProvider ResolveProvider(
        Engine engine,
        Func<string, ICompressionProvider> factory,
        string format
    )
    {
        try
        {
            return factory(format);
        }
        catch (NotSupportedException)
        {
            TypeErrorHelper.Throw(engine, $"Unsupported compression format: {format}.");
            return null;
        }
        catch (Exception e)
        {
            TypeErrorHelper.Throw(engine, $"Failed to create provider: {e.Message}");
            return null;
        }
    }

    public ReadableStreamInstance Readable => _transformStream.Readable;

    public WritableStreamInstance Writable => _transformStream.Writable;
}
