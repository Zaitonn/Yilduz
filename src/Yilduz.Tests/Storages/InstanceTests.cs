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
        Assert.Equal("Storage", Engine.Evaluate($"{storageName}.constructor.name").AsString());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldWorkWithInstanceof(string storageName)
    {
        Assert.True(Engine.Evaluate($"{storageName} instanceof Storage").AsBoolean());
        Assert.True(Engine.Evaluate($"{storageName} instanceof Object").AsBoolean());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHaveCorrectToString(string storageName)
    {
        Assert.Equal("[object Storage]", Engine.Evaluate($"{storageName}.toString()"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHaveAllMethods(string storageName)
    {
        Assert.Equal("function", Engine.Evaluate($"typeof {storageName}.getItem"));
        Assert.Equal("function", Engine.Evaluate($"typeof {storageName}.setItem"));
        Assert.Equal("function", Engine.Evaluate($"typeof {storageName}.removeItem"));
        Assert.Equal("function", Engine.Evaluate($"typeof {storageName}.clear"));
        Assert.Equal("function", Engine.Evaluate($"typeof {storageName}.key"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHaveLengthProperty(string storageName)
    {
        Assert.Equal("number", Engine.Evaluate($"typeof {storageName}.length").AsString());
    }

    [Fact]
    public void ShouldThrowErrorOnStorageConstruction()
    {
        var exception = Assert.Throws<JavaScriptException>(() =>
        {
            Engine.Execute("new Storage();");
        });

        Assert.Contains("Storage", exception.Message);
    }
}
