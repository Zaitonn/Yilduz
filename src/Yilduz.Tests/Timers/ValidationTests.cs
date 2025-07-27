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
        Assert.Throws<JavaScriptException>(() => Engine.Execute($"{method}();"));
    }

    [Theory]
    [InlineData("setTimeout")]
    [InlineData("setInterval")]
    public void ShouldNotThrowWhenAnyArgIsGiven(string method)
    {
        Assert.Equal(1, Engine.Evaluate($"{method}(null)"));
    }

    [Theory]
    [InlineData("setTimeout")]
    [InlineData("setInterval")]
    public void CanAcceptStringAsTimeout(string method)
    {
        Engine.Execute(
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
        Engine.Execute(
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
        Engine.Execute("const id = setTimeout(() => {}, 100);");
        var id = Engine.Evaluate("id").AsNumber();
        Assert.True(id > 0);
    }

    [Fact]
    public void SetIntervalShouldReturnTimerId()
    {
        Engine.Execute("const id = setInterval(() => {}, 100);");
        var id = Engine.Evaluate("id").AsNumber();
        Assert.True(id > 0);
    }

    [Fact]
    public void ShouldGenerateUniqueIds()
    {
        Engine.Execute(
            """
            const id1 = setTimeout(() => {}, 100);
            const id2 = setTimeout(() => {}, 100);
            const id3 = setInterval(() => {}, 100);
            """
        );

        var id1 = Engine.Evaluate("id1").AsNumber();
        var id2 = Engine.Evaluate("id2").AsNumber();
        var id3 = Engine.Evaluate("id3").AsNumber();

        Assert.NotEqual(id1, id2);
        Assert.NotEqual(id2, id3);
        Assert.NotEqual(id1, id3);
    }
}
