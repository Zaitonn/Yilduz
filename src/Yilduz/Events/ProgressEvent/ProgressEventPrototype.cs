using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Utils;

namespace Yilduz.Events.ProgressEvent;

internal sealed class ProgressEventPrototype : ObjectInstance
{
    private static readonly string LengthComputableName = nameof(
            ProgressEventInstance.LengthComputable
        )
        .ToJsStyleName();
    private static readonly string LoadedName = nameof(ProgressEventInstance.Loaded)
        .ToJsStyleName();
    private static readonly string TotalName = nameof(ProgressEventInstance.Total).ToJsStyleName();

    public ProgressEventPrototype(Engine engine, ProgressEventConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(ProgressEvent));
        FastSetProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(
            LengthComputableName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    LengthComputableName.ToJsGetterName(),
                    GetLengthComputable
                ),
                set: null,
                enumerable: true,
                configurable: true
            )
        );

        FastSetProperty(
            LoadedName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, LoadedName.ToJsGetterName(), GetLoaded),
                set: null,
                enumerable: true,
                configurable: true
            )
        );

        FastSetProperty(
            TotalName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, TotalName.ToJsGetterName(), GetTotal),
                set: null,
                enumerable: true,
                configurable: true
            )
        );
    }

    private JsValue GetLengthComputable(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ProgressEventInstance>().LengthComputable;
    }

    private JsValue GetLoaded(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ProgressEventInstance>().Loaded;
    }

    private JsValue GetTotal(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ProgressEventInstance>().Total;
    }
}
