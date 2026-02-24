using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Response;

public sealed class PrototypeTests : TestBase
{
    [Fact]
    public void ShouldBeInstanceofResponse()
    {
        Execute("const res = new Response();");

        Assert.True(Evaluate("res instanceof Response").AsBoolean());
        Assert.True(Evaluate("res instanceof Object").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Execute("const res = new Response();");

        Assert.Equal(
            "[object Response]",
            Evaluate("Object.prototype.toString.call(res)").AsString()
        );
    }

    [Fact]
    public void ShouldExposeMethods()
    {
        Execute("const res = new Response('hello');");

        Assert.Equal("function", Evaluate("typeof res.clone").AsString());
        Assert.Equal("function", Evaluate("typeof res.text").AsString());
        Assert.Equal("function", Evaluate("typeof res.json").AsString());
        Assert.Equal("function", Evaluate("typeof res.arrayBuffer").AsString());
        Assert.Equal("function", Evaluate("typeof res.blob").AsString());
        Assert.Equal("function", Evaluate("typeof res.bytes").AsString());
    }

    [Fact]
    public void ShouldExposeGetters()
    {
        Execute("const res = new Response();");

        Assert.Equal("string", Evaluate("typeof res.type").AsString());
        Assert.Equal("string", Evaluate("typeof res.url").AsString());
        Assert.Equal("boolean", Evaluate("typeof res.redirected").AsString());
        Assert.Equal("number", Evaluate("typeof res.status").AsString());
        Assert.Equal("boolean", Evaluate("typeof res.ok").AsString());
        Assert.Equal("string", Evaluate("typeof res.statusText").AsString());
        Assert.Equal("boolean", Evaluate("typeof res.bodyUsed").AsString());
    }

    [Fact]
    public void ShouldExposeHeadersAsHeadersInstance()
    {
        Execute("const res = new Response();");

        Assert.True(Evaluate("res.headers instanceof Headers").AsBoolean());
    }

    [Fact]
    public void ShouldHaveGettersOnPrototype()
    {
        Execute("const res = new Response();");

        // The getter should be on Object.getPrototypeOf(res), not on res itself
        Assert.True(
            Evaluate(
                    "Object.getOwnPropertyDescriptor(Object.getPrototypeOf(res), 'status') !== undefined"
                )
                .AsBoolean()
        );
        Assert.True(
            Evaluate(
                    "Object.getOwnPropertyDescriptor(Object.getPrototypeOf(res), 'ok') !== undefined"
                )
                .AsBoolean()
        );
        Assert.True(
            Evaluate(
                    "Object.getOwnPropertyDescriptor(Object.getPrototypeOf(res), 'headers') !== undefined"
                )
                .AsBoolean()
        );
    }

    [Fact]
    public void ShouldExposeStaticMethods()
    {
        Assert.Equal("function", Evaluate("typeof Response.error").AsString());
        Assert.Equal("function", Evaluate("typeof Response.redirect").AsString());
        Assert.Equal("function", Evaluate("typeof Response.json").AsString());
    }

    [Fact]
    public void ShouldHaveCorrectConstructorName()
    {
        Execute("const res = new Response();");

        Assert.Equal("Response", Evaluate("res.constructor.name").AsString());
    }

}
