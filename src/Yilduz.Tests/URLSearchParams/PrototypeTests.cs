using Jint;
using Xunit;

namespace Yilduz.Tests.URLSearchParams;

public sealed class PrototypeTests : TestBase
{
    [Fact]
    public void ShouldHaveCorrectPrototype()
    {
        Execute("const params = new URLSearchParams();");
        var constructorName = Evaluate("params.constructor.name").AsString();

        Assert.Equal("URLSearchParams", constructorName);
    }

    [Fact]
    public void ShouldHaveAllMethods()
    {
        Execute("const params = new URLSearchParams();");

        Assert.Equal("function", Evaluate("typeof params.append"));
        Assert.Equal("function", Evaluate("typeof params.delete"));
        Assert.Equal("function", Evaluate("typeof params.get"));
        Assert.Equal("function", Evaluate("typeof params.getAll"));
        Assert.Equal("function", Evaluate("typeof params.has"));
        Assert.Equal("function", Evaluate("typeof params.set"));
        Assert.Equal("function", Evaluate("typeof params.sort"));
        Assert.Equal("function", Evaluate("typeof params.toString"));
    }

    [Fact]
    public void ShouldHaveSizeProperty()
    {
        Execute("const params = new URLSearchParams();");
        var sizeType = Evaluate("typeof params.size");
        Assert.Equal("number", sizeType);
    }

    [Fact]
    public void SizePropertyShouldBeReadOnly()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('foo', 'bar');");
        var originalSize = Evaluate("params.size").AsNumber();

        Execute("params.size = 999;");
        var newSize = Evaluate("params.size").AsNumber();

        Assert.Equal(originalSize, newSize);
        Assert.Equal(1, newSize);
    }

    [Fact]
    public void ShouldWorkWithInstanceof()
    {
        Execute("const params = new URLSearchParams();");
        var isInstanceOfURLSearchParams = Engine
            .Evaluate("params instanceof URLSearchParams")
            .AsBoolean();
        var isInstanceOfObject = Evaluate("params instanceof Object").AsBoolean();

        Assert.True(isInstanceOfURLSearchParams);
        Assert.True(isInstanceOfObject);
    }
}
