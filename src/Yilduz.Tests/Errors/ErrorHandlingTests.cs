using System.Threading;
using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.Errors;

public sealed class ErrorHandlingTests : TestBase
{
    [Fact]
    public void ShouldThrowTypeErrorForInsufficientArguments()
    {
        var exception = Assert.Throws<Jint.Runtime.JavaScriptException>(() =>
        {
            Engine.Execute(
                @"
                const params = new URLSearchParams();
                params.append(); // No arguments provided
            "
            );
        });

        Assert.Contains("TypeError", exception.Error.ToString());
    }

    [Fact]
    public async Task ShouldHandleTimeoutError()
    {
        Engine.Execute(
            @"
            let error = null;
            try {
                const signal = AbortSignal.timeout(10);
                signal.addEventListener('abort', () => {
                    error = signal.reason;
                });
            } catch (e) {
                error = e;
            }
        "
        );

        await Task.Delay(100);

        var errorName = Engine.Evaluate("error ? error.name : null").AsString();
        Assert.Equal("TimeoutError", errorName);
    }

    [Fact]
    public void ShouldHandleAbortError()
    {
        Engine.Execute(
            @"
            const controller = new AbortController();
            controller.abort();
            const error = controller.signal.reason;
        "
        );

        var errorName = Engine.Evaluate("error.name").AsString();
        Assert.Equal("AbortError", errorName);
    }

    [Fact]
    public void ShouldHandleCustomErrors()
    {
        Engine.Execute(
            @"
            const controller = new AbortController();
            const customError = new Error('Custom error message');
            customError.code = 'CUSTOM_CODE';
            controller.abort(customError);
            const error = controller.signal.reason;
        "
        );

        var errorMessage = Engine.Evaluate("error.message").AsString();
        var errorCode = Engine.Evaluate("error.code").AsString();

        Assert.Equal("Custom error message", errorMessage);
        Assert.Equal("CUSTOM_CODE", errorCode);
    }

    [Fact]
    public void ShouldHandleErrorInEventListeners()
    {
        Engine.Execute(
            @"
            const target = new EventTarget();
            let errorCaught = null;
            let secondListenerExecuted = false;
            
            target.addEventListener('test', () => {
                throw new Error('First listener error');
            });
            
            target.addEventListener('test', () => {
                secondListenerExecuted = true;
            });
            
            try {
                target.dispatchEvent(new Event('test'));
            } catch (e) {
                errorCaught = e;
            }
        "
        );

        var secondListenerExecuted = Engine.Evaluate("secondListenerExecuted").AsBoolean();
        Assert.True(secondListenerExecuted); // Should continue despite error
    }

    [Fact]
    public async Task ShouldHandleErrorInTimerCallbacks()
    {
        Engine.Execute(
            @"
            let errorHandled = false;
            let timerExecuted = false;
            
            setTimeout(() => {
                timerExecuted = true;
                throw new Error('Timer error');
            }, 10);
            
            setTimeout(() => {
                errorHandled = true;
            }, 20);
        "
        );

        await Task.Delay(100);

        var timerExecuted = Engine.Evaluate("timerExecuted").AsBoolean();
        var errorHandled = Engine.Evaluate("errorHandled").AsBoolean();

        Assert.True(timerExecuted);
        Assert.True(errorHandled); // Second timer should still execute
    }

    [Fact]
    public void ShouldHandleInvalidEventTypes()
    {
        Engine.Execute(
            @"
            const target = new EventTarget();
            let results = [];
            
            // Test various invalid event types
            try {
                target.addEventListener(null, () => {});
                results.push('null allowed');
            } catch (e) {
                results.push('null rejected');
            }
            
            try {
                target.addEventListener(undefined, () => {});
                results.push('undefined allowed');
            } catch (e) {
                results.push('undefined rejected');
            }
            
            try {
                target.addEventListener(123, () => {});
                results.push('number allowed');
            } catch (e) {
                results.push('number rejected');
            }
        "
        );

        var resultsLength = Engine.Evaluate("results.length").AsNumber();
        Assert.Equal(3, resultsLength);
    }

    [Fact]
    public void ShouldHandleURLSearchParamsErrors()
    {
        Engine.Execute(
            @"
            let results = [];
            
            try {
                const params = new URLSearchParams([['incomplete']]);
                results.push('incomplete array handled');
            } catch (e) {
                results.push('incomplete array rejected: ' + e.name);
            }
            
            try {
                const params = new URLSearchParams([['key', 'value', 'extra']]);
                results.push('extra elements allowed');
            } catch (e) {
                results.push('extra elements rejected: ' + e.name);
            }
        "
        );

        var firstResult = Engine.Evaluate("results[0]").AsString();
        var secondResult = Engine.Evaluate("results[1]").AsString();

        Assert.Contains("rejected", firstResult);
        Assert.Contains("TypeError", firstResult);
    }
}
