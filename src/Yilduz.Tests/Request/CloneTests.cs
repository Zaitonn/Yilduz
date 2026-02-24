using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Request;

public sealed class CloneTests : TestBase
{
    [Fact]
    public void ShouldReturnNewRequestInstance()
    {
        Execute("const req = new Request('https://example.com'); const cloned = req.clone();");

        Assert.True(Evaluate("cloned instanceof Request").AsBoolean());
        Assert.True(Evaluate("cloned !== req").AsBoolean());
    }

    [Fact]
    public void ShouldPreserveUrl()
    {
        Execute(
            """
            const req = new Request('https://example.com/resource');
            const cloned = req.clone();
            """
        );

        Assert.Equal(Evaluate("req.url").AsString(), Evaluate("cloned.url").AsString());
    }

    [Fact]
    public void ShouldPreserveMethod()
    {
        Execute(
            """
            const req = new Request('https://example.com', {
                method: 'DELETE'
            });
            const cloned = req.clone();
            """
        );

        Assert.Equal("DELETE", Evaluate("cloned.method").AsString());
    }

    [Fact]
    public void ShouldPreserveHeaders()
    {
        Execute(
            """
            const req = new Request('https://example.com', {
                headers: { 'X-Custom': 'test-value', 'Accept': 'application/json' }
            });
            const cloned = req.clone();
            """
        );

        Assert.Equal("test-value", Evaluate("cloned.headers.get('x-custom')").AsString());
        Assert.Equal("application/json", Evaluate("cloned.headers.get('accept')").AsString());
    }

    [Fact]
    public void HeadersShouldBeIndependent()
    {
        Execute(
            """
            const req = new Request('https://example.com', {
                headers: { 'X-Original': 'yes' }
            });
            const cloned = req.clone();
            cloned.headers.set('X-Cloned', 'added');
            """
        );

        // Original should not have the header added to the clone
        Assert.True(Evaluate("req.headers.get('x-cloned') === null").AsBoolean());
        // Clone should have it
        Assert.Equal("added", Evaluate("cloned.headers.get('x-cloned')").AsString());
    }

    [Fact]
    public void ShouldPreserveMode()
    {
        Execute(
            """
            const req = new Request('https://example.com', { mode: 'cors' });
            const cloned = req.clone();
            """
        );

        Assert.Equal("cors", Evaluate("cloned.mode").AsString());
    }

    [Fact]
    public void ShouldPreserveCredentials()
    {
        Execute(
            """
            const req = new Request('https://example.com', { credentials: 'include' });
            const cloned = req.clone();
            """
        );

        Assert.Equal("include", Evaluate("cloned.credentials").AsString());
    }

    [Fact]
    public void ShouldPreserveCache()
    {
        Execute(
            """
            const req = new Request('https://example.com', { cache: 'no-store' });
            const cloned = req.clone();
            """
        );

        Assert.Equal("no-store", Evaluate("cloned.cache").AsString());
    }

    [Fact]
    public void ShouldPreserveRedirect()
    {
        Execute(
            """
            const req = new Request('https://example.com', { redirect: 'manual' });
            const cloned = req.clone();
            """
        );

        Assert.Equal("manual", Evaluate("cloned.redirect").AsString());
    }

    [Fact]
    public void ShouldPreserveIntegrity()
    {
        Execute(
            """
            const req = new Request('https://example.com', { integrity: 'sha256-abc123' });
            const cloned = req.clone();
            """
        );

        Assert.Equal("sha256-abc123", Evaluate("cloned.integrity").AsString());
    }

    [Fact]
    public void ShouldPreserveKeepalive()
    {
        Execute(
            """
            const req = new Request('https://example.com', { keepalive: true });
            const cloned = req.clone();
            """
        );

        Assert.True(Evaluate("cloned.keepalive").AsBoolean());
    }

    [Fact]
    public void ShouldPreserveDuplex()
    {
        Execute(
            """
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'data'
            });
            const cloned = req.clone();
            """
        );

        Assert.Equal("half", Evaluate("cloned.duplex").AsString());
    }

    [Fact]
    public void BodyShouldBeReadableFromClone()
    {
        Execute(
            """
            var clonedText;
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'hello clone'
            });
            const cloned = req.clone();
            async function readClone() { clonedText = await cloned.text(); }
            """
        );

        Evaluate("readClone()").UnwrapIfPromise();
        Assert.Equal("hello clone", Evaluate("clonedText").AsString());
    }

    [Fact]
    public void BodyShouldBeReadableFromBothOriginalAndClone()
    {
        Execute(
            """
            var originalText;
            var clonedText;
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'shared body'
            });
            const cloned = req.clone();
            async function readBoth() {
                originalText = await req.text();
                clonedText = await cloned.text();
            }
            """
        );

        Evaluate("readBoth()").UnwrapIfPromise();
        Assert.Equal("shared body", Evaluate("originalText").AsString());
        Assert.Equal("shared body", Evaluate("clonedText").AsString());
    }

    [Fact]
    public void BodyUsedShouldBeFalseOnCloneBeforeConsuming()
    {
        Execute(
            """
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'data'
            });
            const cloned = req.clone();
            """
        );

        Assert.False(Evaluate("cloned.bodyUsed").AsBoolean());
    }

    [Fact]
    public void ConsumingOriginalBodyShouldNotMarkCloneAsUsed()
    {
        Execute(
            """
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'data'
            });
            const cloned = req.clone();
            async function consumeOriginal() { await req.text(); }
            """
        );

        Evaluate("consumeOriginal()").UnwrapIfPromise();

        Assert.True(Evaluate("req.bodyUsed").AsBoolean());
        Assert.False(Evaluate("cloned.bodyUsed").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenBodyAlreadyUsed()
    {
        Execute(
            """
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'data'
            });
            async function consume() { await req.text(); }
            """
        );

        Evaluate("consume()").UnwrapIfPromise();

        Assert.Throws<JavaScriptException>(() => Evaluate("req.clone()"));
    }

    [Fact]
    public void ShouldAllowCloningRequestWithNoBody()
    {
        Execute(
            """
            const req = new Request('https://example.com');
            const cloned = req.clone();
            """
        );

        Assert.True(Evaluate("cloned instanceof Request").AsBoolean());
        Assert.True(Evaluate("cloned.body === null").AsBoolean());
        Assert.False(Evaluate("cloned.bodyUsed").AsBoolean());
    }

    [Fact]
    public void ShouldAllowCloningAClonedRequest()
    {
        Execute(
            """
            const req = new Request('https://example.com/path', { method: 'PATCH' });
            const clone1 = req.clone();
            const clone2 = clone1.clone();
            """
        );

        Assert.True(Evaluate("clone2 instanceof Request").AsBoolean());
        Assert.Equal("PATCH", Evaluate("clone2.method").AsString());
        Assert.Equal("https://example.com/path", Evaluate("clone2.url").AsString());
    }

    [Fact]
    public void CloneSignalShouldBeAbortedWhenOriginalSignalIsAborted()
    {
        Execute(
            """
            const controller = new AbortController();
            const req = new Request('https://example.com', { signal: controller.signal });
            const cloned = req.clone();
            controller.abort();
            """
        );

        Assert.True(Evaluate("cloned.signal.aborted").AsBoolean());
    }

    [Fact]
    public void CloneSignalShouldNotBeAbortedInitially()
    {
        Execute(
            """
            const controller = new AbortController();
            const req = new Request('https://example.com', { signal: controller.signal });
            const cloned = req.clone();
            """
        );

        Assert.False(Evaluate("cloned.signal.aborted").AsBoolean());
    }
}
