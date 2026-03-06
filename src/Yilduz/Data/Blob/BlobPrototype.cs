using System;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;

namespace Yilduz.Data.Blob;

internal sealed class BlobPrototype : PrototypeBase<BlobInstance>
{
    public BlobPrototype(Engine engine, BlobConstructor constructor)
        : base(engine, nameof(Blob), constructor)
    {
        RegisterProperty("size", blob => blob.Size);
        RegisterProperty("type", blob => blob.Type);

        RegisterMethod("text", Text);
        RegisterMethod("stream", Stream);
        RegisterMethod("arrayBuffer", ArrayBuffer);
        RegisterMethod("bytes", Bytes);
        RegisterMethod("slice", Slice);
    }

    private JsValue Text(BlobInstance blob, JsValue[] arguments)
    {
        return QueueBlobTask(() => blob.Text());
    }

    private JsValue Stream(BlobInstance blob, JsValue[] arguments)
    {
        return blob.Stream();
    }

    private JsValue ArrayBuffer(BlobInstance blob, JsValue[] arguments)
    {
        return QueueBlobTask(blob.ArrayBuffer);
    }

    private JsValue Bytes(BlobInstance blob, JsValue[] arguments)
    {
        return QueueBlobTask(blob.Bytes);
    }

    private static JsValue Slice(BlobInstance blob, JsValue[] arguments)
    {
        var start = arguments.At(0).IsUndefined() ? 0 : (int)arguments.At(0).AsNumber();
        var end = arguments.At(1).IsUndefined() ? null : (int?)arguments.At(1).AsNumber();
        var contentType = arguments.At(2).IsUndefined() ? string.Empty : arguments.At(2).AsString();

        return blob.Slice(start, end, contentType);
    }

    private JsValue QueueBlobTask(Func<JsValue> work)
    {
        var promise = Engine.Advanced.RegisterPromise();
        var eventLoop = Engine.GetWebApiIntrinsics().EventLoop;

        eventLoop.QueueMacrotask(() =>
        {
            try
            {
                promise.Resolve(work());
            }
            catch (JavaScriptException ex)
            {
                promise.Reject(ex.Error);
            }
            catch (Exception ex)
            {
                promise.Reject(Engine.Intrinsics.Error.Construct(ex.Message));
            }
        });

        return promise.Promise;
    }
}
