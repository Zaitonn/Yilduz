using System;
using System.Runtime.CompilerServices;
using Jint;

namespace Yilduz;

/// <summary>
/// Extension methods for the Jint engine.
/// </summary>
public static class EngineExtensions
{
    private static readonly ConditionalWeakTable<Engine, WebApiIntrinsics> EngineTable = new();

    /// <summary>
    /// Initializes the Jint engine with Web API intrinsics.
    /// </summary>
    public static Engine InitializeWebApi(this Engine engine, Options options)
    {
        if (EngineTable.TryGetValue(engine, out _))
        {
            return engine;
        }

        var webApiIntrinsics = new WebApiIntrinsics(engine, options);
        EngineTable.Add(engine, webApiIntrinsics);
        return engine;
    }

    /// <summary>
    /// Retrieves the Web API intrinsics for the given Jint engine.
    /// </summary>
    public static WebApiIntrinsics GetWebApiIntrinsics(this Engine engine)
    {
        return EngineTable.TryGetValue(engine, out var webApiIntrinsics)
            ? webApiIntrinsics
            : throw new InvalidOperationException(
                "Web API intrinsics not initialized. Call InitializeWebApi first."
            );
    }
}
