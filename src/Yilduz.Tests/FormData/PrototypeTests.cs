using Jint;
using Xunit;

namespace Yilduz.Tests.FormData;

public sealed class PrototypeTests : TestBase
{
    [Fact]
    public void ShouldHaveCorrectPrototype()
    {
        Execute("const formData = new FormData();");
        var constructorName = Evaluate("formData.constructor.name").AsString();

        Assert.Equal("FormData", constructorName);
    }

    [Fact]
    public void ShouldHaveAllMethods()
    {
        Execute("const formData = new FormData();");

        Assert.Equal("function", Evaluate("typeof formData.append"));
        Assert.Equal("function", Evaluate("typeof formData.delete"));
        Assert.Equal("function", Evaluate("typeof formData.get"));
        Assert.Equal("function", Evaluate("typeof formData.getAll"));
        Assert.Equal("function", Evaluate("typeof formData.has"));
        Assert.Equal("function", Evaluate("typeof formData.set"));
        Assert.Equal("function", Evaluate("typeof formData.entries"));
        Assert.Equal("function", Evaluate("typeof formData.keys"));
        Assert.Equal("function", Evaluate("typeof formData.values"));
        Assert.Equal("function", Evaluate("typeof formData.forEach"));
    }

    [Fact]
    public void ShouldWorkWithInstanceof()
    {
        Execute("const formData = new FormData();");
        var isInstanceOfFormData = Evaluate("formData instanceof FormData").AsBoolean();
        var isInstanceOfObject = Evaluate("formData instanceof Object").AsBoolean();

        Assert.True(isInstanceOfFormData);
        Assert.True(isInstanceOfObject);
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Execute("const formData = new FormData();");
        var toStringTag = Evaluate("Object.prototype.toString.call(formData)").AsString();

        Assert.Equal("[object FormData]", toStringTag);
    }
}
