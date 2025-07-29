using Jint;
using Jint.Native;
using Jint.Native.Promise;

namespace Yilduz.Utils;

internal static class PromiseHelper
{
    public static ManualPromise CreateResolvedPromise(Engine engine, JsValue value)
    {
        var manualPromise = engine.Advanced.RegisterPromise();
        manualPromise.Resolve(value);
        return manualPromise;
    }

    public static ManualPromise CreateRejectedPromise(Engine engine, JsValue reason)
    {
        var manualPromise = engine.Advanced.RegisterPromise();
        manualPromise.Reject(reason);
        return manualPromise;
    }
}
