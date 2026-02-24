using System;
using System.Linq;
using System.Threading;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime;
using Yilduz.Extensions;

namespace Yilduz.Services;

internal sealed class TimerProvider(Engine engine, Options options, EventLoop eventLoop)
{
    private int _timerDepth;

    public JsValue SetTimeout(JsValue _, JsValue[] arguments)
    {
        options.CancellationToken.ThrowIfCancellationRequested();
        return StartTimer(false, arguments);
    }

    public JsValue SetInterval(JsValue _, JsValue[] arguments)
    {
        options.CancellationToken.ThrowIfCancellationRequested();
        return StartTimer(true, arguments);
    }

    public JsValue Clear(JsValue _, JsValue[] arguments)
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        if (arguments.Length == 0)
        {
            return JsValue.Undefined;
        }

        var id = arguments[0];
        if (!id.IsNumber())
        {
            return JsValue.Undefined;
        }

        eventLoop.Cancel((long)id.AsNumber());

        return JsValue.Undefined;
    }

    private long StartTimer(bool repeat, JsValue[] arguments)
    {
        arguments.EnsureCount(engine, 1, repeat ? "setInterval" : "setTimeout", null);

        var firstArgument = arguments[0];
        var handler = firstArgument as Function;
        var args = handler is not null ? arguments.Skip(2).ToArray() : [];
        var code = handler is null ? firstArgument.ToString() : null;

        var timeout =
            arguments.Length > 1
                ? arguments[1].IsNumber()
                    ? (int)arguments[1].AsNumber()
                    : int.TryParse(arguments[1].ToString(), out var parsed)
                        ? parsed
                        : 0
                : 0;

        if (timeout < 0)
        {
            timeout = 0;
        }

        if (Volatile.Read(ref _timerDepth) >= 5 && timeout < 4)
        {
            timeout = 4;
        }

        return eventLoop.ScheduleTimer(BuildCallback(handler, code, args), timeout, repeat);
    }

    private Action BuildCallback(Function? handler, string? code, JsValue[] args)
    {
        return () =>
        {
            Interlocked.Increment(ref _timerDepth);

            try
            {
                FastExecute(handler, code, args);
            }
            finally
            {
                Interlocked.Decrement(ref _timerDepth);
            }
        };
    }

    private void FastExecute(Function? handler, string? code, JsValue[] arguments)
    {
        if (handler is not null)
        {
            Execute(handler, arguments);
        }
        else if (!string.IsNullOrEmpty(code))
        {
            Execute(code);
        }
    }

    private void Execute(Function function, JsValue[] arguments)
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        bool entered = false;
        try
        {
            Monitor.TryEnter(engine, options.WaitingTimeout, ref entered);

            if (entered)
            {
                function.Call(JsValue.Undefined, arguments);
            }
            else
            {
                throw new TimeoutException();
            }
        }
        finally
        {
            if (entered)
            {
                Monitor.Exit(engine);
            }
        }
    }

    private void Execute(string code)
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        bool entered = false;
        try
        {
            Monitor.TryEnter(engine, options.WaitingTimeout, ref entered);

            if (entered)
            {
                engine.Execute(code);
            }
            else
            {
                throw new TimeoutException();
            }
        }
        finally
        {
            if (entered)
            {
                Monitor.Exit(engine);
            }
        }
    }
}
