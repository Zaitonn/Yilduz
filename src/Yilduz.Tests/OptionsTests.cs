using System;
using System.Threading;
using Jint;
using Xunit;

namespace Yilduz.Tests;

public static class OptionsTests
{
    [Fact]
    public static void ShouldThrowWhenNull()
    {
        var engine = new Engine();
        Assert.Throws<ArgumentNullException>(() => engine.InitializeWebApi(null!));
    }

    [Fact]
    public static void ShouldThrowWhenEmpty()
    {
        var engine = new Engine();
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                engine.InitializeWebApi(
                    new()
                    {
                        WaitingTimeout = TimeSpan.FromSeconds(-1),
                        CancellationToken = CancellationToken.None,
                    }
                )
        );
    }
}
