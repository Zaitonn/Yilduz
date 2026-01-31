using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Timers;

public sealed class ValidationTests : TestBase
{
    [Theory]
    [InlineData("setTimeout")]
    [InlineData("setInterval")]
    public void ShouldThrowWhenGivenEmptyArg(string method)
    {
        Assert.Throws<JavaScriptException>(() => Execute($"{method}();"));
    }

    [Theory]
    [InlineData("setTimeout")]
    [InlineData("setInterval")]
    public void ShouldNotThrowWhenAnyArgIsGiven(string method)
    {
        Assert.Equal(1, Evaluate($"{method}(null)"));
    }

    [Theory]
    [InlineData("setTimeout")]
    [InlineData("setInterval")]
    public void CanAcceptStringAsTimeout(string method)
    {
        Execute(
            $"""
            {method}("", 100);
            {method}("", "100");
            """
        );
    }

    [Theory]
    [InlineData("setTimeout")]
    [InlineData("setInterval")]
    public void ShouldNotThrowWhenObjectDoesntHaveToStringMethod(string method)
    {
        Execute(
            $$"""
            {{method}}({
                foo: function() {
                    return "bar";
                }
            }, 100);
            """
        );
    }

    [Fact]
    public void SetTimeoutShouldReturnTimerId()
    {
        Execute("const id = setTimeout(() => {}, 100);");
        var id = Evaluate("id").AsNumber();
        Assert.True(id > 0);
    }

    [Fact]
    public void SetIntervalShouldReturnTimerId()
    {
        Execute("const id = setInterval(() => {}, 100);");
        var id = Evaluate("id").AsNumber();
        Assert.True(id > 0);
    }

    [Fact]
    public void ShouldGenerateUniqueIds()
    {
        Execute(
            """
            const id1 = setTimeout(() => {}, 100);
            const id2 = setTimeout(() => {}, 100);
            const id3 = setInterval(() => {}, 100);
            """
        );

        var id1 = Evaluate("id1").AsNumber();
        var id2 = Evaluate("id2").AsNumber();
        var id3 = Evaluate("id3").AsNumber();

        Assert.NotEqual(id1, id2);
        Assert.NotEqual(id2, id3);
        Assert.NotEqual(id1, id3);
    }
}
