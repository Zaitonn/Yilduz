using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Storages;

public sealed class InstanceTests : TestBase
{
    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHaveCorrectPrototype(string storageName)
    {
        Assert.Equal("Storage", Evaluate($"{storageName}.constructor.name").AsString());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldWorkWithInstanceof(string storageName)
    {
        Assert.True(Evaluate($"{storageName} instanceof Storage").AsBoolean());
        Assert.True(Evaluate($"{storageName} instanceof Object").AsBoolean());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHaveCorrectToString(string storageName)
    {
        Assert.Equal("[object Storage]", Evaluate($"{storageName}.toString()"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHaveAllMethods(string storageName)
    {
        Assert.Equal("function", Evaluate($"typeof {storageName}.getItem"));
        Assert.Equal("function", Evaluate($"typeof {storageName}.setItem"));
        Assert.Equal("function", Evaluate($"typeof {storageName}.removeItem"));
        Assert.Equal("function", Evaluate($"typeof {storageName}.clear"));
        Assert.Equal("function", Evaluate($"typeof {storageName}.key"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHaveLengthProperty(string storageName)
    {
        Assert.Equal("number", Evaluate($"typeof {storageName}.length").AsString());
    }

    [Fact]
    public void ShouldThrowErrorOnStorageConstruction()
    {
        var exception = Assert.Throws<JavaScriptException>(() =>
        {
            Execute("new Storage();");
        });

        Assert.Contains("Storage", exception.Message);
    }
}
