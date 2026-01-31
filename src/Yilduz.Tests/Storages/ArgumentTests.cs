using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Storages;

public sealed class ArgumentTests : TestBase
{
    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldThrowWhenProvidingInvalidArguments(string storageName)
    {
        var exception = Assert.Throws<JavaScriptException>(
            () => Execute($"{storageName}.setItem()")
        );
        Assert.Equal(
            "Failed to execute 'setItem' on 'Storage': 2 arguments required, but only 0 present.",
            exception.Message
        );

        exception = Assert.Throws<JavaScriptException>(() => Execute($"{storageName}.getItem()"));
        Assert.Equal(
            "Failed to execute 'getItem' on 'Storage': 1 argument required, but only 0 present.",
            exception.Message
        );

        exception = Assert.Throws<JavaScriptException>(
            () => Execute($"{storageName}.removeItem()")
        );
        Assert.Equal(
            "Failed to execute 'removeItem' on 'Storage': 1 argument required, but only 0 present.",
            exception.Message
        );

        exception = Assert.Throws<JavaScriptException>(() => Execute($"{storageName}.key()"));
        Assert.Equal(
            "Failed to execute 'key' on 'Storage': 1 argument required, but only 0 present.",
            exception.Message
        );
    }
}
