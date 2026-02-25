using System.Threading;
using Jint;
using Jint.Native;
using Jint.Native.Promise;
using Jint.Runtime;
using Yilduz.Network.Headers;
using Yilduz.Network.Request;
using Yilduz.Network.Response;

namespace Yilduz.Network.Fetch;

internal sealed class FetchProvider(Engine engine, WebApiIntrinsics webApiIntrinsics)
{
    /// <summary>
    /// https://fetch.spec.whatwg.org/#dom-global-fetch
    /// </summary>
    public JsValue Fetch(JsValue thisValue, JsValue[] arguments)
    {
        var input = arguments.At(0);
        var init = arguments.At(1);

        // Step 1. Let p be a new promise.
        var promise = engine.Advanced.RegisterPromise();

        RequestInstance requestObject;

        try
        {
            // Step 2. Let requestObject be the result of invoking the initial value of
            // Request as constructor with input and init as arguments.
            requestObject = webApiIntrinsics.Request.Create(input, init);
        }
        catch (JavaScriptException ex)
        {
            promise.Reject(ex.Error);
            return promise.Promise;
        }

        // Step 3. Let request be requestObject's request.
        var request = requestObject.RequestConcept;

        // Step 4. If requestObject's signal is aborted, then:
        if (requestObject.Signal.Aborted)
        {
            AbortFetchCall(promise, request, responseObject: null, requestObject.Signal.Reason);
            return promise.Promise;
        }

        // Step 8. Let responseObject be null.
        ResponseInstance? responseObject = null;

        // Step 10. Let locallyAborted be false.
        var locallyAborted = false;

        // Step 11. Let controller be null (will be set after calling fetch).
        FetchController controller = default;

        // CancellationTokenSource wired to the AbortSignal so that the HttpClient
        // request is cancelled when the JS signal fires.
        var cts = CancellationTokenSource.CreateLinkedTokenSource(
            webApiIntrinsics.Options.CancellationToken
        );

        // Step 12. Add the following abort steps to requestObject's signal.
        requestObject.Signal.Abort += (_, _) =>
        {
            // Step 12.1. Set locallyAborted to true.
            locallyAborted = true;

            // Step 12.2. Assert: controller is non-null. (always true here)

            // Step 12.3. Abort controller with requestObject's signal's abort reason.
            controller.Abort(requestObject.Signal.Reason);

            // Cancel the underlying HttpClient request.
            cts.Cancel();

            // Step 12.4. Abort the fetch() call.
            AbortFetchCall(promise, request, responseObject, requestObject.Signal.Reason);
        };

        // Step 13. Set controller to the result of calling fetch given request and
        // processResponse given response being these steps:
        controller = FetchImplementation.Fetch(
            engine,
            webApiIntrinsics,
            webApiIntrinsics.Options,
            webApiIntrinsics.EventLoop,
            request,
            processResponse: response =>
            {
                // Step 13.1. If locallyAborted is true, then abort these steps.
                if (locallyAborted)
                {
                    return;
                }

                // Step 13.2. If response's aborted flag is set, then:
                if (response.AbortedFlag)
                {
                    var deserializedError = controller.DeserializeAbortReason(
                        controller.SerializedAbortReason ?? JsValue.Null
                    );
                    AbortFetchCall(promise, request, responseObject, deserializedError);
                    return;
                }

                // Step 13.3. If response is a network error, then reject p with a TypeError.
                if (response.Type == ResponseType.Error)
                {
                    promise.Reject(
                        engine.Intrinsics.TypeError.Construct("Failed to fetch: network error")
                    );
                    return;
                }

                // Step 13.4. Set responseObject to the result of creating a Response object,
                // given response, "immutable", and relevantRealm.
                responseObject = webApiIntrinsics.Response.Create(response, Guard.Immutable);

                // Step 13.5. Resolve p with responseObject.
                promise.Resolve(responseObject);
            },
            processResponseEndOfBody: null,
            cancellationToken: cts.Token
        );

        // Step 14. Return p.
        return promise.Promise;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#abort-fetch
    /// </summary>
    private static void AbortFetchCall(
        ManualPromise promise,
        RequestConcept request,
        ResponseInstance? responseObject,
        JsValue error
    )
    {
        // Step 1. Reject promise with error.
        // (no-op if promise is already fulfilled)
        try
        {
            promise.Reject(error);
        }
        catch { }

        // Step 2. If request's body is non-null and is readable, then cancel it.
        // (ReadableStream cancel is a JS concern handled implicitly by Jint GC)

        // Step 3. If responseObject is null, then return.
        if (responseObject is null)
        {
            return;
        }

        // Step 4. Let response be responseObject's response.
        // Step 5. If response's body is non-null and is readable, then error it.
        var responseBody = responseObject.ResponseConcept.Body?.Stream;
        if (responseBody is not null && !responseBody.Disturbed && !responseBody.Locked)
        {
            try
            {
                responseBody.ErrorInternal(error);
            }
            catch { }
        }
    }
}
