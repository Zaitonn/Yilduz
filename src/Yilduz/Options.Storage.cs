using System;
using Jint;
using Yilduz.Storages.Storage;

namespace Yilduz;

public sealed partial class Options
{
    public StorageOptions Storage { get; init; } = new();

    public sealed class StorageOptions
    {
        internal StorageOptions()
        {
            LocalStorageFactory = engine => new StorageInstance(engine);
            SessionStorageFactory = engine => new StorageInstance(engine);
        }

        public Func<Engine, StorageInstance> LocalStorageFactory { get; set; }
        public Func<Engine, StorageInstance> SessionStorageFactory { get; set; }
    }
}
