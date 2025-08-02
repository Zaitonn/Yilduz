using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Streams.QueuingStrategy;

internal abstract class QueuingStrategyPrototype<T> : ObjectInstance
    where T : QueuingStrategyInstance
{
    private static readonly string SizeName = nameof(Size).ToJsStyleName();
    private static readonly string HighWaterMarkName = nameof(QueuingStrategyInstance.HighWaterMark)
        .ToJsStyleName();

    private readonly string _name;

    protected QueuingStrategyPrototype(Engine engine, string name, Constructor constructor)
        : base(engine)
    {
        _name = name;

        Set(GlobalSymbolRegistry.ToStringTag, _name);
        SetOwnProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(SizeName, new(new ClrFunction(Engine, SizeName, Size), false, false, true));
        FastSetProperty(
            HighWaterMarkName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, HighWaterMarkName.ToJsGetterName(), GetHighWaterMark),
                set: null,
                false,
                true
            )
        );
    }

    private JsValue GetHighWaterMark(JsValue thisObject, JsValue[] arguments)
    {
        var queuingStrategy = thisObject.EnsureThisObject<T>();
        return queuingStrategy.HighWaterMark;
    }

    private JsValue Size(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, "", _name);
        var queuingStrategy = thisObject.EnsureThisObject<T>();

        return queuingStrategy.Size(arguments[0]);
    }
}
