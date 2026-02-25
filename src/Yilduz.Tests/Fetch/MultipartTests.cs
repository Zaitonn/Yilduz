using System;
using System.IO;
using Jint;
using Xunit;

namespace Yilduz.Tests.Fetch;

public sealed class MultipartTests : FetchTestBase
{
    [Fact]
    public void ShouldUploadMultipartFormDataText()
    {
        string? contentType = null;
        string? body = null;

        MapRoute(
            "POST",
            "/multipart-text",
            async ctx =>
            {
                contentType = ctx.Request.ContentType;
                using var reader = new StreamReader(ctx.Request.InputStream);
                body = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                const fd = new FormData();
                fd.append('field1', 'value1');
                await fetch('{{BaseUrl}}multipart-text', {
                    method: 'POST',
                    body: fd
                });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.NotNull(contentType);
        Assert.NotNull(body);
        Assert.Contains("multipart/form-data", contentType);

        var boundaryParts = contentType!.Split(
            "boundary=",
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        Assert.Equal(2, boundaryParts.Length);
        var boundary = boundaryParts[1];
        Assert.Contains($"--{boundary}\r\n", body);
        Assert.Contains("Content-Disposition: form-data; name=\"field1\"", body);
        Assert.Contains("value1", body);
        Assert.EndsWith($"--{boundary}--\r\n", body);
    }

    [Fact]
    public void ShouldUploadMultipartFormDataWithFile()
    {
        string? contentType = null;
        string? body = null;

        MapRoute(
            "POST",
            "/multipart-file",
            async ctx =>
            {
                contentType = ctx.Request.ContentType;
                using var reader = new StreamReader(ctx.Request.InputStream);
                body = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                const fd = new FormData();
                const blob = new Blob(['hello file'], { type: 'text/plain' });
                fd.append('file', blob, 'hello.txt');
                await fetch('{{BaseUrl}}multipart-file', {
                    method: 'POST',
                    body: fd
                });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.NotNull(contentType);
        Assert.NotNull(body);
        Assert.Contains("multipart/form-data", contentType);

        var boundaryParts = contentType!.Split(
            "boundary=",
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        Assert.Equal(2, boundaryParts.Length);
        var boundary = boundaryParts[1];
        Assert.Contains($"--{boundary}\r\n", body);
        Assert.Contains(
            "Content-Disposition: form-data; name=\"file\"; filename=\"hello.txt\"",
            body
        );
        Assert.Contains("Content-Type: text/plain", body);
        Assert.Contains("hello file", body);
        Assert.EndsWith($"--{boundary}--\r\n", body);
    }
}
