using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.URLSearchParams;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateEmptyURLSearchParams()
    {
        Execute("const params = new URLSearchParams();");
        var size = Evaluate("params.size").AsNumber();
        Assert.Equal(0, size);
    }

    [Fact]
    public void ShouldCreateFromString()
    {
        Execute("const params = new URLSearchParams('foo=bar&baz=qux');");
        var size = Evaluate("params.size").AsNumber();
        Assert.Equal(2, size);

        var foo = Evaluate("params.get('foo')").AsString();
        var baz = Evaluate("params.get('baz')").AsString();
        Assert.Equal("bar", foo);
        Assert.Equal("qux", baz);
    }

    [Fact]
    public void ShouldCreateFromStringWithQuestionMark()
    {
        Execute("const params = new URLSearchParams('?foo=bar&baz=qux');");
        var size = Evaluate("params.size").AsNumber();
        Assert.Equal(2, size);

        var foo = Evaluate("params.get('foo')").AsString();
        var baz = Evaluate("params.get('baz')").AsString();
        Assert.Equal("bar", foo);
        Assert.Equal("qux", baz);
    }

    [Fact]
    public void ShouldCreateFromEmptyString()
    {
        Execute("const params = new URLSearchParams('');");
        var size = Evaluate("params.size").AsNumber();
        Assert.Equal(0, size);
    }

    [Fact]
    public void ShouldCreateFromObject()
    {
        Execute("const params = new URLSearchParams({ foo: 'bar', baz: 'qux' });");
        var size = Evaluate("params.size").AsNumber();
        Assert.Equal(2, size);

        var foo = Evaluate("params.get('foo')").AsString();
        var baz = Evaluate("params.get('baz')").AsString();
        Assert.Equal("bar", foo);
        Assert.Equal("qux", baz);
    }

    [Fact]
    public void ShouldCreateFromArray()
    {
        Execute("const params = new URLSearchParams([['foo', 'bar'], ['baz', 'qux']]);");
        var size = Evaluate("params.size").AsNumber();
        Assert.Equal(2, size);

        var foo = Evaluate("params.get('foo')").AsString();
        var baz = Evaluate("params.get('baz')").AsString();
        Assert.Equal("bar", foo);
        Assert.Equal("qux", baz);
    }

    [Fact]
    public void ShouldThrowForInvalidArray()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new URLSearchParams([{}]);"));
        Assert.Throws<JavaScriptException>(() => Execute("new URLSearchParams([['foo']]);"));
        Assert.Throws<JavaScriptException>(
            () => Execute("new URLSearchParams([['foo', '1', '2']]);")
        );
    }

    [Fact]
    public void ShouldHandleUrlEncoding()
    {
        Execute("const params = new URLSearchParams('foo=hello%20world&bar=%3D%26');");
        var foo = Evaluate("params.get('foo')").AsString();
        var bar = Evaluate("params.get('bar')").AsString();
        Assert.Equal("hello world", foo);
        Assert.Equal("=&", bar);
    }

    [Fact]
    public void ShouldHandleParametersWithoutValues()
    {
        Execute("const params = new URLSearchParams('foo&bar=baz');");
        var size = Evaluate("params.size").AsNumber();
        Assert.Equal(2, size);

        var foo = Evaluate("params.get('foo')").AsString();
        var bar = Evaluate("params.get('bar')").AsString();
        Assert.Equal("", foo);
        Assert.Equal("baz", bar);
    }

    [Fact]
    public void ShouldAcceptURLSearchParamsInstance()
    {
        Execute(
            """
            const params = new URLSearchParams(
                new URLSearchParams({
                    foo: 'bar',
                    baz: 'qux'
                })
            );
            """
        );
        var size = Evaluate("params.size").AsNumber();
        Assert.Equal(2, size);

        var foo = Evaluate("params.get('foo')").AsString();
        var baz = Evaluate("params.get('baz')").AsString();
        Assert.Equal("bar", foo);
        Assert.Equal("qux", baz);
    }
}
