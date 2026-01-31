using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStream;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldThrowRangeErrorForUnderlyingSinkTypeProperty()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new WritableStream({ type: 'bytes' });"));
    }

    [Fact]
    public void ShouldCreateWritableStreamWithoutTypeProperty()
    {
        Execute("const stream = new WritableStream({ write() {} });");
        Assert.True(Evaluate("stream instanceof WritableStream").AsBoolean());
    }
}
