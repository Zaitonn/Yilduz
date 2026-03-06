using System;
using Jint;
using Jint.Native.Object;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Console;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/console
/// </summary>
public sealed partial class ConsoleInstance : ObjectInstance, IConsole
{
    private readonly Lazy<IConsole> _console;

    internal ConsoleInstance(Engine engine, Lazy<IConsole> console)
        : base(engine)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        Configure();
    }

    private void Configure()
    {
        this.FastSetMethod("assert", Assert);
        this.FastSetMethod("clear", Clear);
        this.FastSetMethod("count", Count);
        this.FastSetMethod("countReset", CountReset);
        this.FastSetMethod("debug", Debug);
        this.FastSetMethod("dir", Dir);
        this.FastSetMethod("dirxml", Dirxml);
        this.FastSetMethod("error", Error);
        this.FastSetMethod("group", Group);
        this.FastSetMethod("groupCollapsed", GroupCollapsed);
        this.FastSetMethod("groupEnd", GroupEnd);
        this.FastSetMethod("info", Info);
        this.FastSetMethod("log", Log);
        this.FastSetMethod("table", Table);
        this.FastSetMethod("time", Time);
        this.FastSetMethod("timeEnd", TimeEnd);
        this.FastSetMethod("timeLog", TimeLog);
        this.FastSetMethod("timeStamp", TimeStamp);
        this.FastSetMethod("trace", Trace);
        this.FastSetMethod("warn", Warn);
    }
}
