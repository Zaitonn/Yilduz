using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Yilduz.Services;

internal sealed class EventLoop
{
    private readonly Options _options;

    private readonly object _lock = new();

    private readonly HashSet<long> _activeTimerIds = [];

    private readonly Queue<Action> _macrotasks = new();

    private readonly Queue<Action> _microtasks = new();

    private readonly SortedSet<TimerRegistration> _timers = new(new TimerComparer());

    private readonly SemaphoreSlim _signal = new(0, int.MaxValue);

    private long _timerId;

    public EventLoop(Options options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        _ = Task.Run(RunAsync, options.CancellationToken);
    }

    public long ScheduleTimer(Action callback, int delay, bool repeat)
    {
        var registration = new TimerRegistration(
            Interlocked.Increment(ref _timerId),
            repeat,
            delay,
            DateTimeOffset.UtcNow.AddMilliseconds(delay),
            callback
        );

        lock (_lock)
        {
            _activeTimerIds.Add(registration.Id);
            _timers.Add(registration);
        }

        _signal.Release();

        return registration.Id;
    }

    public void Cancel(long id)
    {
        lock (_lock)
        {
            _activeTimerIds.Remove(id);
            _timers.RemoveWhere(timer => timer.Id == id);
        }

        _signal.Release();
    }

    public void QueueMicrotask(Action action)
    {
        lock (_lock)
        {
            _microtasks.Enqueue(action);
        }

        _signal.Release();
    }

    public void QueueMacrotask(Action action)
    {
        lock (_lock)
        {
            _macrotasks.Enqueue(action);
        }

        _signal.Release();
    }

    private async Task RunAsync()
    {
        var token = _options.CancellationToken;

        while (!token.IsCancellationRequested)
        {
            Action? macrotask = null;
            var hasPendingMicrotasks = false;
            TimerRegistration? dueTimer = null;
            var wait = Timeout.InfiniteTimeSpan;
            var now = DateTimeOffset.UtcNow;

            lock (_lock)
            {
                if (_macrotasks.Count > 0)
                {
                    macrotask = _macrotasks.Dequeue();
                }
                else if (_microtasks.Count > 0)
                {
                    hasPendingMicrotasks = true;
                }
                else if (_timers.Count > 0)
                {
                    var next = _timers.Min;

                    if (next is not null)
                    {
                        var remaining = next.NextRun - now;

                        if (remaining <= TimeSpan.Zero)
                        {
                            _timers.Remove(next);
                            dueTimer = next;
                        }
                        else
                        {
                            wait = remaining;
                        }
                    }
                }
            }

            if (macrotask is not null)
            {
                RunMacrotask(macrotask);
                continue;
            }

            if (hasPendingMicrotasks)
            {
                FlushMicrotasks();
                continue;
            }

            if (dueTimer is not null)
            {
                RunTimer(dueTimer);
                continue;
            }

            await WaitAsync(wait, token).ConfigureAwait(false);
        }
    }

    private void RunTimer(TimerRegistration timer)
    {
        if (IsCancelled(timer.Id))
        {
            return;
        }

        try
        {
            timer.Callback();
        }
        catch { }

        if (
            timer.Repeat
            && !IsCancelled(timer.Id)
            && !_options.CancellationToken.IsCancellationRequested
        )
        {
            timer.NextRun = DateTimeOffset.UtcNow.AddMilliseconds(timer.Delay);

            lock (_lock)
            {
                _timers.Add(timer);
            }

            _signal.Release();
        }
        else if (!timer.Repeat)
        {
            lock (_lock)
            {
                _activeTimerIds.Remove(timer.Id);
            }
        }

        FlushMicrotasks();
    }

    private void RunMacrotask(Action action)
    {
        try
        {
            action();
        }
        catch { }

        FlushMicrotasks();
    }

    private void FlushMicrotasks()
    {
        while (true)
        {
            Action? microtask = null;

            lock (_lock)
            {
                if (_microtasks.Count == 0)
                {
                    break;
                }

                microtask = _microtasks.Dequeue();
            }

            try
            {
                microtask();
            }
            catch { }
        }
    }

    private Task WaitAsync(TimeSpan delay, CancellationToken token)
    {
        if (delay == Timeout.InfiniteTimeSpan)
        {
            return _signal.WaitAsync(token);
        }

#if NETSTANDARD
        var clamped =
            delay.TotalMilliseconds < 0 ? 0
            : delay.TotalMilliseconds > int.MaxValue ? int.MaxValue
            : (int)delay.TotalMilliseconds;
#else
        var clamped = Math.Clamp((int)delay.TotalMilliseconds, 0, int.MaxValue);
#endif

        return _signal.WaitAsync(TimeSpan.FromMilliseconds(clamped), token);
    }

    private bool IsCancelled(long id)
    {
        lock (_lock)
        {
            return !_activeTimerIds.Contains(id);
        }
    }

    private sealed class TimerRegistration(
        long id,
        bool repeat,
        int delay,
        DateTimeOffset nextRun,
        Action callback
    )
    {
        public long Id { get; } = id;

        public bool Repeat { get; } = repeat;

        public int Delay { get; } = delay;

        public DateTimeOffset NextRun { get; set; } = nextRun;

        public Action Callback { get; } = callback;
    }

    private sealed class TimerComparer : IComparer<TimerRegistration>
    {
        public int Compare(TimerRegistration? x, TimerRegistration? y)
        {
            if (x is null && y is null)
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            var result = x.NextRun.CompareTo(y.NextRun);

            return result != 0 ? result : x.Id.CompareTo(y.Id);
        }
    }
}
