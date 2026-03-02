using Jint;
using Jint.Native;
using Xunit;

namespace Yilduz.Tests.XMLHttpRequest;

public sealed class PrototypeTests : TestBase
{
    [Fact]
    public void ShouldExposePrototypeMembers()
    {
        Execute("const xhr = new XMLHttpRequest();");

        Assert.True(Evaluate("xhr instanceof XMLHttpRequest").AsBoolean());
        Assert.True(Evaluate("xhr instanceof Object").AsBoolean());
        Assert.Equal("function", Evaluate("typeof xhr.open").AsString());
        Assert.Equal("function", Evaluate("typeof xhr.send").AsString());
        Assert.Equal("function", Evaluate("typeof xhr.abort").AsString());
        Assert.Equal("function", Evaluate("typeof xhr.setRequestHeader").AsString());
        Assert.Equal("function", Evaluate("typeof xhr.getResponseHeader").AsString());
        Assert.Equal("function", Evaluate("typeof xhr.getAllResponseHeaders").AsString());
        Assert.True(Evaluate("xhr.upload instanceof XMLHttpRequestUpload").AsBoolean());
        Assert.Equal("function", Evaluate("typeof xhr.overrideMimeType").AsString());
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Execute("const xhr = new XMLHttpRequest();");

        Assert.Equal(
            "[object XMLHttpRequest]",
            Evaluate("Object.prototype.toString.call(xhr)").AsString()
        );
    }

    [Fact]
    public void ShouldExposeDefaultStateAndProperties()
    {
        Execute("const xhr = new XMLHttpRequest();");

        Assert.Equal(0, Evaluate("xhr.readyState"));
        Assert.Equal(0, Evaluate("xhr.status"));
        Assert.Equal(string.Empty, Evaluate("xhr.responseText").AsString());
        Assert.Equal(string.Empty, Evaluate("xhr.responseType").AsString());
        Assert.False(Evaluate("xhr.withCredentials").AsBoolean());
        Assert.Equal(0, Evaluate("xhr.timeout"));
    }

    [Fact]
    public void ShouldExposeEventTargetApis()
    {
        Execute("const xhr = new XMLHttpRequest();");

        Assert.Equal("function", Evaluate("typeof xhr.addEventListener").AsString());
        Assert.Equal("function", Evaluate("typeof xhr.removeEventListener").AsString());
        Assert.Equal("function", Evaluate("typeof xhr.dispatchEvent").AsString());
    }

    [Fact]
    public void ShouldExposeReadyStateConstantsOnConstructor()
    {
        Execute("const ctor = XMLHttpRequest;");

        Assert.Equal(0, Evaluate("ctor.UNSENT"));
        Assert.Equal(1, Evaluate("ctor.OPENED"));
        Assert.Equal(2, Evaluate("ctor.HEADERS_RECEIVED"));
        Assert.Equal(3, Evaluate("ctor.LOADING"));
        Assert.Equal(4, Evaluate("ctor.DONE"));
    }

    [Fact]
    public void ShouldExposeReadyStateConstantsOnPrototype()
    {
        Execute("const proto = XMLHttpRequest.prototype;");

        Assert.Equal(0, Evaluate("proto.UNSENT"));
        Assert.Equal(1, Evaluate("proto.OPENED"));
        Assert.Equal(2, Evaluate("proto.HEADERS_RECEIVED"));
        Assert.Equal(3, Evaluate("proto.LOADING"));
        Assert.Equal(4, Evaluate("proto.DONE"));
    }
}
