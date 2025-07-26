using Xunit;

namespace Yilduz.Tests.Console;

public sealed class ConsoleInstanceTests : TestBase
{
    private readonly MockConsole _mockConsole = new();

    protected override Options GetOptions()
    {
        return new() { CancellationToken = Token, ConsoleFactory = engine => _mockConsole };
    }

    [Fact]
    public void ShouldCallLogWithCorrectArguments()
    {
        Engine.Execute("console.log('hello', 'world');");

        Assert.Single(_mockConsole.LogCalls);
        Assert.Contains("Log:", _mockConsole.LogCalls[0]);
        Assert.Contains("hello", _mockConsole.LogCalls[0]);
        Assert.Contains("world", _mockConsole.LogCalls[0]);
    }

    [Fact]
    public void ShouldCallAssertWithConditionAndMessage()
    {
        Engine.Execute("console.assert(false, 'assertion failed', 'extra info');");

        Assert.Single(_mockConsole.LogCalls);
        Assert.Contains("Assert: False", _mockConsole.LogCalls[0]);
        Assert.Contains("assertion failed", _mockConsole.LogCalls[0]);
    }

    [Fact]
    public void ShouldCallCountWithLabel()
    {
        Engine.Execute("console.count('test-counter');");

        Assert.Single(_mockConsole.LogCalls);
        Assert.Contains("Count: test-counter", _mockConsole.LogCalls[0]);
    }

    [Fact]
    public void ShouldCallCountWithoutLabel()
    {
        Engine.Execute("console.count();");

        Assert.Single(_mockConsole.LogCalls);
        Assert.Contains("Count: default", _mockConsole.LogCalls[0]);
    }

    [Fact]
    public void ShouldCallTimeOperations()
    {
        Engine.Execute(
            @"
            console.time('timer1');
            console.timeLog('timer1', 'checkpoint');
            console.timeEnd('timer1');
        "
        );

        Assert.Equal(3, _mockConsole.LogCalls.Count);
        Assert.Contains("Time: timer1", _mockConsole.LogCalls[0]);
        Assert.Contains("TimeLog: timer1", _mockConsole.LogCalls[1]);
        Assert.Contains("TimeEnd: timer1", _mockConsole.LogCalls[2]);
    }

    [Fact]
    public void ShouldCallGroupOperations()
    {
        Engine.Execute(
            @"
            console.group('group1');
            console.log('inside group');
            console.groupEnd();
        "
        );

        Assert.Equal(3, _mockConsole.LogCalls.Count);
        Assert.Contains("Group: group1", _mockConsole.LogCalls[0]);
        Assert.Contains("Log: inside group", _mockConsole.LogCalls[1]);
        Assert.Contains("GroupEnd", _mockConsole.LogCalls[2]);
    }

    [Fact]
    public void ShouldCallAllLogLevels()
    {
        Engine.Execute(
            @"
            console.debug('debug msg');
            console.info('info msg');
            console.warn('warn msg'); 
            console.error('error msg');
        "
        );

        Assert.Equal(4, _mockConsole.LogCalls.Count);
        Assert.Contains("Debug: debug msg", _mockConsole.LogCalls[0]);
        Assert.Contains("Info: info msg", _mockConsole.LogCalls[1]);
        Assert.Contains("Warn: warn msg", _mockConsole.LogCalls[2]);
        Assert.Contains("Error: error msg", _mockConsole.LogCalls[3]);
    }

    [Fact]
    public void ShouldCallClear()
    {
        Engine.Execute("console.clear();");

        Assert.Single(_mockConsole.LogCalls);
        Assert.Equal("Clear", _mockConsole.LogCalls[0]);
    }

    [Fact]
    public void ShouldCallDir()
    {
        Engine.Execute("console.dir({name: 'test'});");

        Assert.Single(_mockConsole.LogCalls);
        Assert.Contains("Dir:", _mockConsole.LogCalls[0]);
    }

    [Fact]
    public void ShouldCallTrace()
    {
        Engine.Execute("console.trace('trace message');");

        Assert.Single(_mockConsole.LogCalls);
        Assert.Contains("Trace: trace message", _mockConsole.LogCalls[0]);
    }

    [Fact]
    public void ShouldHandleUndefinedArguments()
    {
        Engine.Execute("console.log(undefined, null, true, 42);");

        Assert.Single(_mockConsole.LogCalls);
        Assert.Contains("Log:", _mockConsole.LogCalls[0]);
    }
}
