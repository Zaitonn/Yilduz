using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime;
using Yilduz.Extensions;

namespace Yilduz.Services;

internal sealed class TimerProvider(Engine engine, Options options)
{
    private readonly List<long> _ids = [];

    private long _currentId = 1;

    public JsValue SetTimeout(JsValue thisObject, params JsValue[] arguments)
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        return StartTimer(false, arguments);
    }

    public JsValue SetInterval(JsValue thisObject, params JsValue[] arguments)
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        return StartTimer(true, arguments);
    }

    public JsValue Clear(JsValue thisObject, params JsValue[] arguments)
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

        lock (_ids)
        {
            _ids.Remove((long)arguments[0].AsNumber());
        }

        return JsValue.Undefined;
    }

    private long StartTimer(bool repeat, JsValue[] arguments)
    {
        arguments.EnsureCount(engine, 1, repeat ? "setInterval" : "setTimeout", null);

        var id = Interlocked.Read(ref _currentId);
        Interlocked.Add(ref _currentId, 1);

        _ids.Add(id);

        var firstArgument = arguments[0];
        var handler = firstArgument as Function;
        var args = handler is not null ? arguments.Skip(2).ToArray() : null;
        var code = handler is null
            ? firstArgument.IsString()
                ? firstArgument.AsString()
                : firstArgument.ToString()
            : null;

        var timeout =
            arguments.Length > 1
                ? arguments[1].IsNumber()
                    ? (int)arguments[1].AsNumber()
                    : int.TryParse(arguments[1].ToString(), out var parsed)
                        ? parsed
                        : 0
                : 0;

        if (timeout < 4)
        {
            timeout = 4;
        }

        Task.Run(
            async () =>
            {
                do
                {
                    await Task.Delay(timeout, options.CancellationToken).ConfigureAwait(false);

                    if (options.CancellationToken.IsCancellationRequested || !_ids.Contains(id))
                    {
                        break;
                    }

                    try
                    {
                        FastExecute();
                    }
                    catch { }

                    if (options.CancellationToken.IsCancellationRequested || !_ids.Contains(id))
                    {
                        break;
                    }
                } while (repeat);
            },
            options.CancellationToken
        );

        return id;

        void FastExecute()
        {
            if (handler is not null)
            {
                Execute(handler, args!);
            }
            else if (!string.IsNullOrEmpty(code))
            {
                Execute(code!);
            }
        }
    }

    private void Execute(Function function, JsValue[] arguments)
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        bool entered = false;
        try
        {
            Monitor.TryEnter(function.Engine, options.WaitingTimeout, ref entered);

            if (entered)
            {
                function.Call(JsValue.Undefined, arguments);
            }
        }
        finally
        {
            if (entered)
            {
                Monitor.Exit(function.Engine);
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
