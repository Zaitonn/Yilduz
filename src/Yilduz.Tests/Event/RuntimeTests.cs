using Jint;
using Xunit;

namespace Yilduz.Tests.Event;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void DefaultPreventedShouldBeFalse()
    {
        Engine.Execute("const event = new Event('test');");
        var result = Engine.Evaluate("event.defaultPrevented").AsBoolean();
        Assert.False(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CanBePrevented(bool cancelable)
    {
        Engine.Execute(
            $"const event = new Event('test', {{ cancelable: {cancelable.ToString().ToLowerInvariant()} }});"
        );
        Engine.Evaluate("event.preventDefault();");
        Assert.Equal(Engine.Evaluate("event.defaultPrevented").AsBoolean(), cancelable);
    }
}
