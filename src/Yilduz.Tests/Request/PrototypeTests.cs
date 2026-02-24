using Jint;
using Xunit;

namespace Yilduz.Tests.Request;

public sealed class PrototypeTests : TestBase
{
    [Fact]
    public void ShouldExposePrototypeMembers()
    {
        Execute("const req = new Request('https://example.com');");

        Assert.True(Evaluate("req instanceof Request").AsBoolean());
        Assert.True(Evaluate("req instanceof Object").AsBoolean());
        Assert.Equal("function", Evaluate("typeof req.clone").AsString());
        Assert.True(Evaluate("req.headers instanceof Headers").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Execute("const req = new Request('https://example.com');");

        Assert.Equal(
            "[object Request]",
            Evaluate("Object.prototype.toString.call(req)").AsString()
        );
    }
}
