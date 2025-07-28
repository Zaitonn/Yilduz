using Jint;
using Jint.Native.Object;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Console;

public sealed partial class ConsoleInstance : ObjectInstance, IConsole
{
    private readonly IConsole _console;

    internal ConsoleInstance(Engine engine, IConsole console)
        : base(engine)
    {
        _console = console;
        Configure();
    }

    private void Configure()
    {
        FastSetProperty(
            nameof(Assert).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Assert).ToJsStyleName(), Assert), false, false, true)
        );
        FastSetProperty(
            nameof(Clear).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Clear).ToJsStyleName(), Clear), false, false, true)
        );
        FastSetProperty(
            nameof(Count).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Count).ToJsStyleName(), Count), false, false, true)
        );
        FastSetProperty(
            nameof(CountReset).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(CountReset).ToJsStyleName(), CountReset),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(Debug).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Debug).ToJsStyleName(), Debug), false, false, true)
        );
        FastSetProperty(
            nameof(Dir).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Dir).ToJsStyleName(), Dir), false, false, true)
        );
        FastSetProperty(
            nameof(Dirxml).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Dirxml).ToJsStyleName(), Dirxml), false, false, true)
        );
        FastSetProperty(
            nameof(Error).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Error).ToJsStyleName(), Error), false, false, true)
        );
        FastSetProperty(
            nameof(Group).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Group).ToJsStyleName(), Group), false, false, true)
        );
        FastSetProperty(
            nameof(GroupCollapsed).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(GroupCollapsed).ToJsStyleName(), GroupCollapsed),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(GroupEnd).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(GroupEnd).ToJsStyleName(), GroupEnd),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(Info).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Info).ToJsStyleName(), Info), false, false, true)
        );
        FastSetProperty(
            nameof(Log).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Log).ToJsStyleName(), Log), false, false, true)
        );
        FastSetProperty(
            nameof(Table).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Table).ToJsStyleName(), Table), false, false, true)
        );
        FastSetProperty(
            nameof(Time).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Time).ToJsStyleName(), Time), false, false, true)
        );
        FastSetProperty(
            nameof(TimeEnd).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(TimeEnd).ToJsStyleName(), TimeEnd),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(TimeLog).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(TimeLog).ToJsStyleName(), TimeLog),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(TimeStamp).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(TimeStamp).ToJsStyleName(), TimeStamp),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(Trace).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Trace).ToJsStyleName(), Trace), false, false, true)
        );
        FastSetProperty(
            nameof(Warn).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Warn).ToJsStyleName(), Warn), false, false, true)
        );
    }
}
