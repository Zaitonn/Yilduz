using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HttpMultipartParser;
using Jint;
using Jint.Native;
using Jint.Native.Json;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Data.Blob;
using Yilduz.Data.File;
using Yilduz.Data.FormData;
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
                        // https://url.spec.whatwg.org/#concept-urlencoded-parser

                        // Let sequences be the result of splitting input on 0x26 (&).
                        var input = System.Text.Encoding.UTF8.GetString(bytes);
                        var sequences = input.Split('&');

                        // Let output be an initially empty list of name-value tuples where both name and value hold a string.
                        var output = new List<(string Name, string Value)>();

                        // For each byte sequence bytes in sequences:
                        foreach (var part in sequences)
                        {
                            // If bytes is the empty byte sequence, then continue.
                            if (string.IsNullOrEmpty(part))
                            {
                                continue;
                            }

                            // If bytes contains a 0x3D (=), then let name be the bytes from the start of bytes up to but excluding its first 0x3D (=),
                            // and let value be the bytes, if any, after the first 0x3D (=) up to the end of bytes.
                            // If 0x3D (=) is the first byte, then name will be the empty byte sequence.
                            // If it is the last, then value will be the empty byte sequence.
                            var equalIndex = part.IndexOf('=');
                            string name,
                                value;
                            if (equalIndex >= 0)
                            {
                                name = part[..equalIndex];
                                value = part[(equalIndex + 1)..];
                            }
                            else
                            {
                                // Otherwise, let name be bytes and value be the empty byte sequence.
                                name = part;
                                value = string.Empty;
                            }

                            // Replace any 0x2B (+) in name and value with 0x20 (SP).
                            name = name.Replace('+', ' ');
                            value = value.Replace('+', ' ');

                            // Let nameString and valueString be the result of running UTF-8 decode without BOM on the percent-decoding of name and value, respectively.
                            name = Uri.UnescapeDataString(name);
                            value = Uri.UnescapeDataString(value);

                            output.Add((name, value));
                        }

                        var formData1 = (FormDataInstance)
                            _webApiIntrinsics.FormData.Construct([Undefined], Undefined);

                        formData1.EntryList.AddRange(
                            output.Select(o => (o.Name, (JsValue)o.Value, (string?)null))
                        );
                        return formData1;

                    case "multipart/form-data":
                        try
                        {
                            using var stream = new MemoryStream(bytes);
                            var formDataParser = MultipartFormDataParser.Parse(
                                stream,
                                mimeType.Parameters.TryGetValue("boundary", out var boundary)
                                    ? boundary
                                    : null
                            );

                            var formData2 = (FormDataInstance)
                                _webApiIntrinsics.FormData.Construct([Undefined], Undefined);

                            formData2.EntryList.AddRange(
                                formDataParser.Parameters.Select(p =>
                                    (p.Name, (JsValue)p.Data, (string?)null)
                                )
                            );

                            foreach (var file in formDataParser.Files)
                            {
                                using var ms = new MemoryStream();
                                file.Data.CopyTo(ms);

                                var fileInstance = (FileInstance)
                                    _webApiIntrinsics.File.Construct(
                                        [Undefined, file.FileName],
                                        Undefined
                                    );

                                fileInstance.Value.AddRange(ms.ToArray());
                                formData2.EntryList.Add((file.Name, fileInstance, file.FileName));
                            }

                            return formData2;
                        }
                        catch (Exception ex) when (ex is not JavaScriptException)
                        {
                            TypeErrorHelper.Throw(
                                Engine,
                                "Failed to parse multipart/form-data: " + ex.Message
                            );
                            break;
                        }
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
