using Jint;
using Xunit;

namespace Yilduz.Tests.Headers;

public sealed class PrototypeTests : TestBase
{
    [Fact]
    public void ShouldHaveCorrectPrototype()
    {
        Execute("const headers = new Headers();");
        var constructorName = Evaluate("headers.constructor.name").AsString();

        Assert.Equal("Headers", constructorName);
    }

    [Fact]
    public void ShouldHaveAllMethods()
    {
        Execute("const headers = new Headers();");

        Assert.Equal("function", Evaluate("typeof headers.append"));
        Assert.Equal("function", Evaluate("typeof headers.delete"));
        Assert.Equal("function", Evaluate("typeof headers.get"));
        Assert.Equal("function", Evaluate("typeof headers.has"));
        Assert.Equal("function", Evaluate("typeof headers.set"));
        Assert.Equal("function", Evaluate("typeof headers.entries"));
        Assert.Equal("function", Evaluate("typeof headers.keys"));
        Assert.Equal("function", Evaluate("typeof headers.values"));
        Assert.Equal("function", Evaluate("typeof headers.forEach"));
        Assert.Equal("function", Evaluate("typeof headers.getSetCookie"));
    }

    [Fact]
    public void ShouldWorkWithInstanceof()
    {
        Execute("const headers = new Headers();");
        var isInstanceOfHeaders = Evaluate("headers instanceof Headers").AsBoolean();
        var isInstanceOfObject = Evaluate("headers instanceof Object").AsBoolean();

        Assert.True(isInstanceOfHeaders);
        Assert.True(isInstanceOfObject);
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Execute("const headers = new Headers();");
        var toStringTag = Evaluate("Object.prototype.toString.call(headers)").AsString();

        Assert.Equal("[object Headers]", toStringTag);
    }
}
