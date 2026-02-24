using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Response;

public sealed class CloneTests : TestBase
{
    [Fact]
    public void CloneShouldReturnNewInstance()
    {
        Execute(
            """
            const res = new Response('body');
            const clone = res.clone();
            """
        );

        Assert.False(Evaluate("res === clone").AsBoolean());
        Assert.True(Evaluate("clone instanceof Response").AsBoolean());
        Assert.Equal("body", Evaluate("clone.text()").UnwrapIfPromise().AsString());
    }

    [Fact]
    public void CloneShouldCopyStatusAndHeaders()
    {
        Execute(
            """
            const res = new Response(null, {
                status: 201,
                statusText: 'Created',
                headers: { 'X-Id': '7' }
            });
            const clone = res.clone();
            """
        );

        Assert.Equal(201, Evaluate("clone.status").AsNumber());
        Assert.Equal("Created", Evaluate("clone.statusText").AsString());
        Assert.Equal("7", Evaluate("clone.headers.get('x-id')").AsString());
    }

    [Fact]
    public void CloneShouldAllowBothBodiesToBeConsumedIndependently()
    {
        Execute(
            """
            const res = new Response('original');
            const clone = res.clone();
            """
        );

        var t1 = Evaluate("res.text()").UnwrapIfPromise();
        Assert.Equal("original", t1.AsString());

        var t2 = Evaluate("clone.text()").UnwrapIfPromise();
        Assert.Equal("original", t2.AsString());
    }

    [Fact]
    public void CloneShouldThrowWhenBodyAlreadyConsumed()
    {
        Execute("const res = new Response('data');");

        // consume body
        Evaluate("res.text()").UnwrapIfPromise();

        Assert.Throws<JavaScriptException>(() => Execute("res.clone();"));
    }
}
