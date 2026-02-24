using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Extensions;
using Yilduz.Streams.ReadableByteStreamController;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamBYOBRequest;

/// <summary>
/// https://streams.spec.whatwg.org/#readablestreambyobrequest
/// <br />
/// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamBYOBRequest
/// </summary>
public sealed class ReadableStreamBYOBRequestInstance : ObjectInstance
{
    /// <summary>
    /// https://streams.spec.whatwg.org/#readablestreambyobrequest-controller
    /// </summary>
    internal ReadableByteStreamControllerInstance? Controller { get; private set; }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readablestreambyobrequest-view
    /// </summary>
    internal JsValue ViewSlot { get; private set; }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readablestreambyobrequest
    /// </summary>
    internal ReadableStreamBYOBRequestInstance(
        Engine engine,
        ReadableByteStreamControllerInstance controller,
        JsValue view
    )
        : base(engine)
    {
        Controller = controller;
        ViewSlot = view;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-byob-request-view
    /// <br />
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamBYOBRequest/view
    /// </summary>
    public JsValue View => ViewSlot;

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-byob-request-respond
    /// <br />
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamBYOBRequest/respond
    /// </summary>
    public void Respond(ulong bytesWritten)
    {
        var view = ValidateRespondable();
        var currentByteLength = view.GetByteLength();

        if (bytesWritten > currentByteLength)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Failed to execute 'respond' on 'ReadableStreamBYOBRequest': bytesWritten is out of range."
            );
        }

        Invalidate();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-byob-request-respond-with-new-view
    /// <br />
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamBYOBRequest/respondWithNewView
    /// </summary>
    public void RespondWithNewView(JsValue view)
    {
        var currentView = ValidateRespondable();
        ValidateArrayBufferView(view, "respondWithNewView");

        var currentBuffer = GetBuffer(currentView);
        var newBuffer = GetBuffer(view);

        if (currentBuffer != newBuffer)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Failed to execute 'respondWithNewView' on 'ReadableStreamBYOBRequest': view must share the same backing buffer."
            );
        }

        var currentByteOffset = currentView.GetByteOffset();
        var newByteOffset = view.GetByteOffset();
        if (currentByteOffset != newByteOffset)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Failed to execute 'respondWithNewView' on 'ReadableStreamBYOBRequest': view has an invalid byteOffset."
            );
        }

        var currentByteLength = currentView.GetByteLength();
        var newByteLength = view.GetByteLength();
        if (newByteLength > currentByteLength)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Failed to execute 'respondWithNewView' on 'ReadableStreamBYOBRequest': view has an invalid byteLength."
            );
        }

        Invalidate();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-byob-request-respond
    /// </summary>
    private JsValue ValidateRespondable()
    {
        if (Controller is null)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Failed to execute on 'ReadableStreamBYOBRequest': This BYOB request has been invalidated."
            );
        }

        var view = ViewSlot;
        ValidateArrayBufferView(view, "respond");
        return view;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readablestreambyobrequest-invalidate
    /// </summary>
    internal void Invalidate()
    {
        Controller = null;
        ViewSlot = Null;
    }

    /// <summary>
    /// https://webidl.spec.whatwg.org/#ArrayBufferView
    /// </summary>
    private void ValidateArrayBufferView(JsValue value, string methodName)
    {
        if (!value.IsDataView() && value is not JsTypedArray)
        {
            TypeErrorHelper.Throw(
                Engine,
                $"Failed to execute '{methodName}' on 'ReadableStreamBYOBRequest': parameter 1 is not of type 'ArrayBufferView'."
            );
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readablestreambyobrequest-view
    /// </summary>
    private JsValue GetBuffer(JsValue value)
    {
        var buffer = value.Get("buffer");
        if (!buffer.IsArrayBuffer())
        {
            TypeErrorHelper.Throw(
                Engine,
                "ReadableStreamBYOBRequest view buffer is not a valid ArrayBuffer."
            );
        }

        return buffer;
    }
}
