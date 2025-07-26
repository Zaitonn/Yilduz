using System;
using Yilduz.Storages.Storage;

namespace Yilduz;

public sealed partial class Options
{
    public StorageOptions Storage { get; init; } = new();

    public sealed class StorageOptions
    {
        public Action<StorageInstance>? LocalStorageConfigurator { get; set; }
        public Action<StorageInstance>? SessionStorageConfigurator { get; set; }
    }
}
