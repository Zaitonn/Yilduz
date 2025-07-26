using Jint;
using Xunit;

namespace Yilduz.Tests.EventTarget;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateEventTarget()
    {
        Engine.Execute("const target = new EventTarget();");
        Assert.Equal("EventTarget", Engine.Evaluate("target.constructor.name"));
    }

    [Fact]
    public void ShouldWorkWithInstanceof()
    {
        Engine.Execute("const target = new EventTarget();");

        Assert.True(Engine.Evaluate("target instanceof EventTarget").AsBoolean());
        Assert.True(Engine.Evaluate("target instanceof Object").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectMethods()
    {
        Engine.Execute("const target = new EventTarget();");

        Assert.Equal("function", Engine.Evaluate("typeof target.addEventListener").AsString());
        Assert.Equal("function", Engine.Evaluate("typeof target.removeEventListener").AsString());
        Assert.Equal("function", Engine.Evaluate("typeof target.dispatchEvent").AsString());
    }
}
