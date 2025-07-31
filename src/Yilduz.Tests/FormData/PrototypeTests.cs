using Jint;
using Xunit;

namespace Yilduz.Tests.FormData;

public sealed class PrototypeTests : TestBase
{
    [Fact]
    public void ShouldHaveCorrectPrototype()
    {
        Engine.Execute("const formData = new FormData();");
        var constructorName = Engine.Evaluate("formData.constructor.name").AsString();

        Assert.Equal("FormData", constructorName);
    }

    [Fact]
    public void ShouldHaveAllMethods()
    {
        Engine.Execute("const formData = new FormData();");

        Assert.Equal("function", Engine.Evaluate("typeof formData.append"));
        Assert.Equal("function", Engine.Evaluate("typeof formData.delete"));
        Assert.Equal("function", Engine.Evaluate("typeof formData.get"));
        Assert.Equal("function", Engine.Evaluate("typeof formData.getAll"));
        Assert.Equal("function", Engine.Evaluate("typeof formData.has"));
        Assert.Equal("function", Engine.Evaluate("typeof formData.set"));
        Assert.Equal("function", Engine.Evaluate("typeof formData.entries"));
        Assert.Equal("function", Engine.Evaluate("typeof formData.keys"));
        Assert.Equal("function", Engine.Evaluate("typeof formData.values"));
        Assert.Equal("function", Engine.Evaluate("typeof formData.forEach"));
    }

    [Fact]
    public void ShouldWorkWithInstanceof()
    {
        Engine.Execute("const formData = new FormData();");
        var isInstanceOfFormData = Engine.Evaluate("formData instanceof FormData").AsBoolean();
        var isInstanceOfObject = Engine.Evaluate("formData instanceof Object").AsBoolean();

        Assert.True(isInstanceOfFormData);
        Assert.True(isInstanceOfObject);
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Engine.Execute("const formData = new FormData();");
        var toStringTag = Engine.Evaluate("Object.prototype.toString.call(formData)").AsString();

        Assert.Equal("[object FormData]", toStringTag);
    }
}
