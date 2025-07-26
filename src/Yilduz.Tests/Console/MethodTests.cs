using Xunit;

namespace Yilduz.Tests.Console;

public sealed class ConsoleTests : TestBase
{
    [Fact]
    public void ShouldHaveConsoleObject()
    {
        Assert.Equal("object", Engine.Evaluate("typeof console"));
    }

    [Theory]
    [InlineData("assert")]
    [InlineData("clear")]
    [InlineData("count")]
    [InlineData("countReset")]
    [InlineData("debug")]
    [InlineData("dir")]
    [InlineData("dirxml")]
    [InlineData("error")]
    [InlineData("group")]
    [InlineData("groupCollapsed")]
    [InlineData("groupEnd")]
    [InlineData("info")]
    [InlineData("log")]
    [InlineData("table")]
    [InlineData("time")]
    [InlineData("timeEnd")]
    [InlineData("timeLog")]
    [InlineData("timeStamp")]
    [InlineData("trace")]
    [InlineData("warn")]
    public void ShouldHaveAllConsoleMethods(string methodName)
    {
        Assert.Equal("function", Engine.Evaluate($"typeof console.{methodName}"));
    }

    [Theory]
    [InlineData("console.log('test')")]
    [InlineData("console.log('test', 123, true)")]
    [InlineData("console.assert(true, 'should not fail')")]
    [InlineData("console.assert(false, 'assertion failed')")]
    // [InlineData("console.clear()")] // Always fails in tests environment
    [InlineData("console.count()")]
    [InlineData("console.count('testLabel')")]
    [InlineData("console.countReset('testLabel')")]
    [InlineData("console.debug('debug message')")]
    [InlineData("console.dir({prop: 'value'})")]
    [InlineData("console.dirxml('<div>test</div>')")]
    [InlineData("console.error('error message')")]
    [InlineData("console.info('info message')")]
    [InlineData("console.trace('trace message')")]
    [InlineData("console.warn('warning message')")]
    [InlineData("console.timeStamp('timestamp')")]
    [InlineData("console.group('group1'); console.log('inside group'); console.groupEnd()")]
    [InlineData(
        "console.groupCollapsed('collapsed group'); console.log('inside collapsed group'); console.groupEnd()"
    )]
    [InlineData("console.table([{name: 'John', age: 30}, {name: 'Jane', age: 25}])")]
    [InlineData("console.table([{name: 'John', age: 30}], ['name'])")]
    [InlineData(
        "console.time('timer1'); console.timeLog('timer1', 'checkpoint'); console.timeEnd('timer1')"
    )]
    public void ShouldCallAllConsoleMethods(string jsCode)
    {
        Engine.Evaluate(jsCode);
    }
}
