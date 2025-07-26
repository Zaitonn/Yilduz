using Jint;
using Xunit;

namespace Yilduz.Tests.URLSearchParams;

public sealed class PrototypeTests : TestBase
{
    [Fact]
    public void ShouldHaveCorrectPrototype()
    {
        Engine.Execute("const params = new URLSearchParams();");
        var constructorName = Engine.Evaluate("params.constructor.name").AsString();

        Assert.Equal("URLSearchParams", constructorName);
    }

    [Fact]
    public void ShouldHaveAllMethods()
    {
        Engine.Execute("const params = new URLSearchParams();");

        // Check if all methods exist
        Assert.Equal("function", Engine.Evaluate("typeof params.append"));
        Assert.Equal("function", Engine.Evaluate("typeof params.delete"));
        Assert.Equal("function", Engine.Evaluate("typeof params.get"));
        Assert.Equal("function", Engine.Evaluate("typeof params.getAll"));
        Assert.Equal("function", Engine.Evaluate("typeof params.has"));
        Assert.Equal("function", Engine.Evaluate("typeof params.set"));
        Assert.Equal("function", Engine.Evaluate("typeof params.sort"));
        Assert.Equal("function", Engine.Evaluate("typeof params.toString"));
    }

    [Fact]
    public void ShouldHaveSizeProperty()
    {
        Engine.Execute("const params = new URLSearchParams();");
        var sizeType = Engine.Evaluate("typeof params.size");
        Assert.Equal("number", sizeType);
    }

    [Fact]
    public void SizePropertyShouldBeReadOnly()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('foo', 'bar');");
        var originalSize = Engine.Evaluate("params.size").AsNumber();

        Engine.Execute("params.size = 999;");
        var newSize = Engine.Evaluate("params.size").AsNumber();

        Assert.Equal(originalSize, newSize);
        Assert.Equal(1, newSize);
    }

    [Fact]
    public void ShouldWorkWithInstanceof()
    {
        Engine.Execute("const params = new URLSearchParams();");
        var isInstanceOfURLSearchParams = Engine
            .Evaluate("params instanceof URLSearchParams")
            .AsBoolean();
        var isInstanceOfObject = Engine.Evaluate("params instanceof Object").AsBoolean();

        Assert.True(isInstanceOfURLSearchParams);
        Assert.True(isInstanceOfObject);
    }
}
