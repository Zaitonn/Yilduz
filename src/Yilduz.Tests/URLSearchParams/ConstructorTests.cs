using Jint;
using Xunit;

namespace Yilduz.Tests.URLSearchParams;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateEmptyURLSearchParams()
    {
        Engine.Execute("const params = new URLSearchParams();");
        var size = Engine.Evaluate("params.size").AsNumber();
        Assert.Equal(0, size);
    }

    [Fact]
    public void ShouldCreateFromString()
    {
        Engine.Execute("const params = new URLSearchParams('foo=bar&baz=qux');");
        var size = Engine.Evaluate("params.size").AsNumber();
        Assert.Equal(2, size);

        var foo = Engine.Evaluate("params.get('foo')").AsString();
        var baz = Engine.Evaluate("params.get('baz')").AsString();
        Assert.Equal("bar", foo);
        Assert.Equal("qux", baz);
    }

    [Fact]
    public void ShouldCreateFromStringWithQuestionMark()
    {
        Engine.Execute("const params = new URLSearchParams('?foo=bar&baz=qux');");
        var size = Engine.Evaluate("params.size").AsNumber();
        Assert.Equal(2, size);

        var foo = Engine.Evaluate("params.get('foo')").AsString();
        var baz = Engine.Evaluate("params.get('baz')").AsString();
        Assert.Equal("bar", foo);
        Assert.Equal("qux", baz);
    }

    [Fact]
    public void ShouldCreateFromEmptyString()
    {
        Engine.Execute("const params = new URLSearchParams('');");
        var size = Engine.Evaluate("params.size").AsNumber();
        Assert.Equal(0, size);
    }

    [Fact]
    public void ShouldCreateFromObject()
    {
        Engine.Execute("const params = new URLSearchParams({ foo: 'bar', baz: 'qux' });");
        var size = Engine.Evaluate("params.size").AsNumber();
        Assert.Equal(2, size);

        var foo = Engine.Evaluate("params.get('foo')").AsString();
        var baz = Engine.Evaluate("params.get('baz')").AsString();
        Assert.Equal("bar", foo);
        Assert.Equal("qux", baz);
    }

    [Fact]
    public void ShouldCreateFromArray()
    {
        Engine.Execute("const params = new URLSearchParams([['foo', 'bar'], ['baz', 'qux']]);");
        var size = Engine.Evaluate("params.size").AsNumber();
        Assert.Equal(2, size);

        var foo = Engine.Evaluate("params.get('foo')").AsString();
        var baz = Engine.Evaluate("params.get('baz')").AsString();
        Assert.Equal("bar", foo);
        Assert.Equal("qux", baz);
    }

    [Fact]
    public void ShouldHandleUrlEncoding()
    {
        Engine.Execute("const params = new URLSearchParams('foo=hello%20world&bar=%3D%26');");
        var foo = Engine.Evaluate("params.get('foo')").AsString();
        var bar = Engine.Evaluate("params.get('bar')").AsString();
        Assert.Equal("hello world", foo);
        Assert.Equal("=&", bar);
    }

    [Fact]
    public void ShouldHandleParametersWithoutValues()
    {
        Engine.Execute("const params = new URLSearchParams('foo&bar=baz');");
        var size = Engine.Evaluate("params.size").AsNumber();
        Assert.Equal(2, size);

        var foo = Engine.Evaluate("params.get('foo')").AsString();
        var bar = Engine.Evaluate("params.get('bar')").AsString();
        Assert.Equal("", foo);
        Assert.Equal("baz", bar);
    }

    [Fact]
    public void ShouldAcceptURLSearchParamsInstance()
    {
        Engine.Execute(
            """
            const params = new URLSearchParams(
                new URLSearchParams({
                    foo: 'bar',
                    baz: 'qux'
                })
            );
            """
        );
        var size = Engine.Evaluate("params.size").AsNumber();
        Assert.Equal(2, size);

        var foo = Engine.Evaluate("params.get('foo')").AsString();
        var baz = Engine.Evaluate("params.get('baz')").AsString();
        Assert.Equal("bar", foo);
        Assert.Equal("qux", baz);
    }
}
