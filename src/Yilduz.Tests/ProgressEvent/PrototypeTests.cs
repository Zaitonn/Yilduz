using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ProgressEvent;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("total")]
    [InlineData("loaded")]
    [InlineData("lengthComputable")]
    public void ShouldHaveCorrectPrototype(string property)
    {
        Assert.True(
            Engine.Evaluate($"ProgressEvent.prototype.hasOwnProperty('{property}')").AsBoolean()
        );
    }

    [Theory]
    [InlineData("ProgressEvent.prototype.total")]
    [InlineData("ProgressEvent.prototype.loaded")]
    [InlineData("ProgressEvent.prototype.lengthComputable")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate(expression));
    }

    [Fact]
    public void ShouldInheritEventPrototype()
    {
        Engine.Execute(
            """
            const progressEvent = new ProgressEvent('test');
            """
        );
        Assert.True(Engine.Evaluate("progressEvent instanceof Event").AsBoolean());
        Assert.True(Engine.Evaluate("progressEvent instanceof ProgressEvent").AsBoolean());
    }
}
