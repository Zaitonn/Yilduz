using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Native.Json;
using Jint.Native.Object;
using Yilduz.Data.Blob;
using Yilduz.Extensions;
using Yilduz.Streams.ReadableStream;
using Yilduz.Streams.ReadableStreamDefaultReader;
using Yilduz.Utils;

namespace Yilduz.Network.Body;

/// <summary>
/// https://fetch.spec.whatwg.org/#body-mixin
/// </summary>
public abstract class BodyInstance : ObjectInstance
{
    private protected readonly WebApiIntrinsics _webApiIntrinsics;

    internal BodyInstance(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine)
    {
        _webApiIntrinsics = webApiIntrinsics;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-body-body
    /// </summary>
    internal protected ReadableStreamInstance? Body => BodyConcept?.Stream;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-body-bodyused
    /// </summary>
    internal protected bool BodyUsed => BodyConcept?.Stream != null && BodyConcept.Stream.Disturbed;

    internal abstract BodyConcept? BodyConcept { get; }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-body-consume-body
    /// </summary>
    protected JsValue ConsumeBody(Func<byte[], JsValue> convertBytesToJSValue)
    {
        // 1. If object is unusable, then return a promise rejected with a TypeError.
        if (
            BodyConcept?.Stream != null
            && (BodyConcept.Stream.Disturbed || BodyConcept.Stream.Locked)
        )
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    Engine.Intrinsics.TypeError.Construct("Body is unusable")
                )
                .Promise;
        }

        // 2. Let promise be a new promise.
        var promise = Engine.Advanced.RegisterPromise();

        // 3. Let errorSteps given error be to reject promise with error.
        void ErrorSteps(JsValue error) => promise.Reject(error);

        // 4. Let successSteps given a byte sequence data be to resolve promise with the result of running convertBytesToJSValue with data. If that threw an exception, then run errorSteps with that exception.
        void SuccessSteps(byte[] data)
        {
            try
            {
                var jsValue = convertBytesToJSValue(data);
                promise.Resolve(jsValue);
            }
            catch (Exception ex)
            {
                ErrorSteps(FromObject(Engine, ex));
            }
        }

        // 5. If object’s body is null, then run successSteps with an empty byte sequence.
        if (BodyConcept?.Stream is null)
        {
            SuccessSteps([]);
            return promise.Promise;
        }

        // 6. Otherwise, fully read object’s body given successSteps, errorSteps, and object’s relevant global object.
        var reader = BodyConcept.Stream.GetReader(Undefined);
        var chunks = new List<byte>();

        void ReadNext()
        {
            ((ReadableStreamDefaultReaderInstance)reader)
                .Read()
                .Then(
                    onFulfilled: result =>
                    {
                        var resultObj = result.AsObject();
                        var done = resultObj.Get("done").AsBoolean();

                        if (done)
                        {
                            SuccessSteps([.. chunks]);

                            if (reader is ReadableStreamDefaultReaderInstance defaultReader)
                            {
                                defaultReader.ReleaseLock();
                            }

                            return result;
                        }

                        var value = resultObj.Get("value");
                        var bytes = value.TryAsBytes();

                        if (bytes is not null)
                        {
                            chunks.AddRange(bytes);
                        }

                        ReadNext();
                        return result;
                    },
                    onRejected: error =>
                    {
                        ErrorSteps(error);

                        if (reader is ReadableStreamDefaultReaderInstance defaultReader)
                        {
                            defaultReader.ReleaseLock();
                        }

                        return error;
                    }
                );
        }

        ReadNext();

        // 7. Return promise.
        return promise.Promise;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-body-arraybuffer
    /// </summary>
    public JsValue ArrayBuffer()
    {
        return ConsumeBody(Engine.Intrinsics.ArrayBuffer.Construct);
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-body-blob
    /// </summary>
    public JsValue Blob()
    {
        return ConsumeBody(bytes =>
        {
            var obj = new JsObject(Engine);
            obj.Set("type", MIMETypeHelper.Get(this)?.Essence);

            var blob = (BlobInstance)_webApiIntrinsics.Blob.Construct([Undefined], Undefined);
            blob.Value.AddRange(bytes);
            return blob;
        });
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-body-formdata
    /// </summary>
    public JsValue FormData()
    {
        return ConsumeBody(bytes =>
        {
            var mimeType = MIMETypeHelper.Get(this);
            if (mimeType is not null)
            {
                switch (mimeType.Essence)
                {
                    case "application/x-www-form-urlencoded":
                        throw new NotImplementedException();

                    case "multipart/form-data":
                        throw new NotImplementedException();
                }
            }

            TypeErrorHelper.Throw(Engine, "Content-Type header is not set or invalid.");
            return Undefined;
        });
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-body-json
    /// </summary>
    public JsValue Json()
    {
        return ConsumeBody(bytes =>
        {
            var jsonString = System.Text.Encoding.UTF8.GetString(bytes);
            return new JsonParser(Engine).Parse(jsonString);
        });
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-body-text
    /// </summary>
    public JsValue Text()
    {
        return ConsumeBody(bytes => System.Text.Encoding.UTF8.GetString(bytes));
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-body-bytes
    /// </summary>
    public JsValue Bytes()
    {
        return ConsumeBody(bytes => Engine.Intrinsics.Uint8Array.Construct(bytes));
    }
}
