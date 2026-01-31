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
        Assert.True(Evaluate($"ProgressEvent.prototype.hasOwnProperty('{property}')").AsBoolean());
    }

    [Theory]
    [InlineData("ProgressEvent.prototype.total")]
    [InlineData("ProgressEvent.prototype.loaded")]
    [InlineData("ProgressEvent.prototype.lengthComputable")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Fact]
    public void ShouldInheritEventPrototype()
    {
        Execute(
            """
            const progressEvent = new ProgressEvent('test');
            """
        );
        Assert.True(Evaluate("progressEvent instanceof Event").AsBoolean());
        Assert.True(Evaluate("progressEvent instanceof ProgressEvent").AsBoolean());
    }
}
