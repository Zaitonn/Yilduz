using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Yilduz.Data.Blob;
using Yilduz.Extensions;
using Yilduz.Network.Body;
using Yilduz.Network.Headers;
using Yilduz.Network.Request;
using Yilduz.Network.Response;
using Yilduz.Services;
using Yilduz.URLs.URL;

namespace Yilduz.Network.Fetch;

/// <summary>
/// Implements the core fetch algorithm.
/// https://fetch.spec.whatwg.org/#concept-fetch
/// </summary>
internal static class FetchImplementation
{
    /// <summary>
    /// A shared <see cref="HttpClient"/> used when no factory is configured and redirect
    /// mode is \"follow\". <see cref="HttpClient"/> is thread-safe for concurrent requests.
    /// </summary>
    private static readonly HttpClient SharedDefaultClient = new();

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-fetch
    /// </summary>
    internal static FetchController Fetch(
        Engine engine,
        WebApiIntrinsics webApiIntrinsics,
        Options options,
        EventLoop eventLoop,
        RequestConcept request,
        Action<ResponseConcept>? processResponse,
        Action<ResponseConcept>? processResponseEndOfBody,
        CancellationToken cancellationToken
    )
    {
        var now = (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var timingInfo = new FetchTimingInfo
        {
            StartTime = now,
            PostRedirectStartTime = now,
            RenderBlocking = request.RenderBlocking,
        };

        var fetchParams = new FetchParams(engine) { Request = request, TimingInfo = timingInfo };

        // Run main fetch in parallel.
        _ = Task.Run(
            async () =>
                await MainFetchAsync(
                        engine,
                        webApiIntrinsics,
                        options,
                        eventLoop,
                        fetchParams,
                        processResponse,
                        processResponseEndOfBody,
                        cancellationToken
                    )
                    .ConfigureAwait(false),
            cancellationToken
        );

        return fetchParams.Controller;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-main-fetch
    /// </summary>
    private static async Task MainFetchAsync(
        Engine engine,
        WebApiIntrinsics webApiIntrinsics,
        Options options,
        EventLoop eventLoop,
        FetchParams fetchParams,
        Action<ResponseConcept>? processResponse,
        Action<ResponseConcept>? processResponseEndOfBody,
        CancellationToken cancellationToken
    )
    {
        var request = fetchParams.Request!;
        ResponseConcept response;

        try
        {
            response = await HttpFetchAsync(
                    engine,
                    webApiIntrinsics,
                    options,
                    eventLoop,
                    request,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            response = ResponseConcept.CreateAbortedNetworkError();
        }
        catch
        {
            response = ResponseConcept.CreateNetworkError();
        }

        eventLoop.QueueMacrotask(() => processResponse?.Invoke(response));
    }

    /// <summary>
    /// Performs the actual HTTP request and converts the result to a
    /// <see cref="ResponseConcept"/>. Runs on a background thread.
    /// <br/>
    /// https://fetch.spec.whatwg.org/#http-fetch
    /// </summary>
    private static async Task<ResponseConcept> HttpFetchAsync(
        Engine engine,
        WebApiIntrinsics webApiIntrinsics,
        Options options,
        EventLoop eventLoop,
        RequestConcept request,
        CancellationToken cancellationToken
    )
    {
        var redirectMode = request.RedirectMode;

        // Choose HttpClient – respect the user-provided factory, otherwise select based on
        // redirect mode so that AllowAutoRedirect can be configured correctly.
        HttpClient httpClient;
        var ownsClient = false;

        if (options.Network.HttpClientFactory is not null)
        {
            httpClient = options.Network.HttpClientFactory();
        }
        else if (string.Equals(redirectMode, Redirect.Follow, StringComparison.Ordinal))
        {
            // Re-use the shared instance for the common \"follow\" case.
            httpClient = SharedDefaultClient;
        }
        else
        {
            // \"error\" or \"manual\" – do not follow redirects automatically.
            httpClient = new HttpClient(
                new HttpClientHandler { AllowAutoRedirect = false },
                disposeHandler: true
            );
            ownsClient = true;
        }

        try
        {
            using var httpRequest = BuildHttpRequest(request);

            using var httpResponse = await httpClient
                .SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (
                !string.Equals(redirectMode, Redirect.Follow, StringComparison.Ordinal)
                && IsRedirectStatus(httpResponse.StatusCode)
            )
            {
                return ResponseConcept.CreateNetworkError();
            }

            // Read the body on the background thread to avoid blocking the JS thread.
            var responseBytes = await httpResponse
                .Content
#if NET5_0_OR_GREATER
                .ReadAsByteArrayAsync(cancellationToken)
#else
                .ReadAsByteArrayAsync()
#endif
                .ConfigureAwait(false);

            return await BuildResponseConceptAsync(
                    engine,
                    webApiIntrinsics,
                    eventLoop,
                    httpResponse,
                    responseBytes,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
        finally
        {
            if (ownsClient)
            {
                httpClient.Dispose();
            }
        }
    }

    /// <summary>
    /// Constructs an <see cref="HttpRequestMessage"/> from a <see cref="RequestConcept"/>.
    /// https://fetch.spec.whatwg.org/#http-network-or-cache-fetch
    /// </summary>
    private static HttpRequestMessage BuildHttpRequest(RequestConcept request)
    {
        var httpMethod = new HttpMethod(request.Method);
        var requestUri = request.CurrentURL.Href;
        var httpRequest = new HttpRequestMessage(httpMethod, requestUri);

        var bodyBytes = GetRequestBodyBytes(request);
        if (bodyBytes is { Length: > 0 })
        {
            httpRequest.Content = new ByteArrayContent(bodyBytes);
        }

        foreach (var header in request.HeaderList)
        {
            // Headers that belong to the content (Content-Type, Content-Length, …)
            // must be added to Content.Headers; all others go on the request.
            if (!httpRequest.Headers.TryAddWithoutValidation(header.Name, header.Value))
            {
                httpRequest.Content ??= new ByteArrayContent(Array.Empty<byte>());
                httpRequest.Content.Headers.TryAddWithoutValidation(header.Name, header.Value);
            }
        }

        return httpRequest;
    }

    /// <summary>
    /// Builds a <see cref="ResponseConcept"/> from the raw HTTP response data.
    /// Body creation (ReadableStream) must happen on the JS thread, so this method
    /// posts the work back via <paramref name="eventLoop"/> and awaits the result.
    /// https://fetch.spec.whatwg.org/#concept-response
    /// </summary>
    private static Task<ResponseConcept> BuildResponseConceptAsync(
        Engine engine,
        WebApiIntrinsics webApiIntrinsics,
        EventLoop eventLoop,
        HttpResponseMessage httpResponse,
        byte[] responseBytes,
        CancellationToken cancellationToken
    )
    {
        // Capture all data we need from the HttpResponseMessage before handing back to the
        // JS thread, because HttpResponseMessage is disposed after this method returns.
        var statusCode = (ushort)httpResponse.StatusCode;
        var reasonPhrase = httpResponse.ReasonPhrase ?? string.Empty;
        var headerList = BuildHeaderList(httpResponse);

        // Capture the final URL after any redirects.
        var finalUri = httpResponse.RequestMessage?.RequestUri;

        var tcs = new TaskCompletionSource<ResponseConcept>();

        eventLoop.QueueMacrotask(() =>
        {
            try
            {
                BodyConcept? bodyConcept = null;
                if (responseBytes.Length > 0)
                {
                    var underlyingSource = new JsObject(engine);
                    var stream = webApiIntrinsics.ReadableStream.Construct(
                        underlyingSource,
                        JsValue.Undefined
                    );
                    stream.Enqueue(engine.Intrinsics.Uint8Array.Construct(responseBytes));
                    stream.Controller.CloseInternal();

                    bodyConcept = new BodyConcept(
                        stream,
                        JsValue.FromObject(engine, responseBytes),
                        responseBytes.LongLength
                    );
                }

                var urlList = new System.Collections.Generic.List<URLInstance>();
                if (finalUri is not null)
                {
                    var parsedUrl = webApiIntrinsics.URL.Parse(finalUri.AbsoluteUri, null);
                    if (!parsedUrl.IsNull())
                    {
                        urlList.Add(parsedUrl);
                    }
                }

                var responseConcept = new ResponseConcept
                {
                    Status = statusCode,
                    StatusMessage = reasonPhrase,
                    HeaderList = headerList,
                    Body = bodyConcept,
                    URLList = urlList,
                };

                tcs.SetResult(responseConcept);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    /// <summary>
    /// Extracts all response headers (including content headers) into a <see cref="HeaderList"/>.
    /// https://fetch.spec.whatwg.org/#concept-response-header-list
    /// </summary>
    private static HeaderList BuildHeaderList(HttpResponseMessage httpResponse)
    {
        var headerList = new HeaderList();

        foreach (var header in httpResponse.Headers)
        {
            foreach (var value in header.Value)
            {
                headerList.Add(new(header.Key, value));
            }
        }

        if (httpResponse.Content is not null)
        {
            foreach (var header in httpResponse.Content.Headers)
            {
                foreach (var value in header.Value)
                {
                    headerList.Add(new(header.Key, value));
                }
            }
        }

        return headerList;
    }

    /// <summary>
    /// Tries to extract a byte array from the request body's source.
    /// Returns null when the body is empty or uses unsupported streaming sources.
    /// https://fetch.spec.whatwg.org/#concept-request-body
    /// </summary>
    private static byte[]? GetRequestBodyBytes(RequestConcept request)
    {
        var body = request.Body;
        if (body is null)
        {
            return null;
        }

        // Fast path: body source is already a byte array (covers string, ArrayBuffer,
        // URLSearchParams body types set by BodyExtractor).
        if (body.Source is not null)
        {
            var bytes = body.Source.TryAsBytes();
            if (bytes is not null)
            {
                return bytes;
            }

            // Blob body type.
            if (body.Source is BlobInstance blob)
            {
                return [.. blob.Value];
            }
        }

        return null;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#redirect-status
    /// </summary>
    private static bool IsRedirectStatus(HttpStatusCode status) =>
        (int)status is 301 or 302 or 303 or 307 or 308;
}
