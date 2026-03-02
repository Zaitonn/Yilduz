using Jint;
using Xunit;

namespace Yilduz.Tests.WebSocket;

public sealed class BaseUrlMissingTests : TestBase
{
    [Fact]
    public void ShouldThrowSyntaxErrorWhenRelativeUrlAndNoBaseUrl()
    {
        Execute(
            """
            let err = null;
            try { new WebSocket('/chat'); }
            catch (e) { err = e; }
            """
        );

        Assert.Equal("SyntaxError", Evaluate("err.name").AsString());
    }
}
