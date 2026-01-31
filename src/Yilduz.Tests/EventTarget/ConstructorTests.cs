using Jint;
using Xunit;

namespace Yilduz.Tests.EventTarget;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateEventTarget()
    {
        Execute("const target = new EventTarget();");
        Assert.Equal("EventTarget", Evaluate("target.constructor.name"));
    }

    [Fact]
    public void ShouldWorkWithInstanceof()
    {
        Execute("const target = new EventTarget();");

        Assert.True(Evaluate("target instanceof EventTarget").AsBoolean());
        Assert.True(Evaluate("target instanceof Object").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectMethods()
    {
        Execute("const target = new EventTarget();");

        Assert.Equal("function", Evaluate("typeof target.addEventListener").AsString());
        Assert.Equal("function", Evaluate("typeof target.removeEventListener").AsString());
        Assert.Equal("function", Evaluate("typeof target.dispatchEvent").AsString());
    }
}
