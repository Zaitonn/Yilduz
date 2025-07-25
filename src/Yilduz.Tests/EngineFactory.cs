using System.Threading;
using Jint;

namespace Yilduz.Tests;

public static class EngineFactory
{
    public static Engine Create(CancellationToken cancellationToken = default)
    {
        var engine = new Engine(
            new Options { Modules = { RegisterRequire = true } }.CancellationToken(
                cancellationToken
            )
        );

        return engine;
    }
}
