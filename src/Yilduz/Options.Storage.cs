using System;
using System.Collections.Generic;
using Yilduz.Storages.Storage;

namespace Yilduz;

public sealed partial class Options
{
    public StorageOptions Storage { get; init; } = new();

    public sealed class StorageOptions
    {
        public StorageConfiguration LocalStorage { get; } = new();
        public StorageConfiguration SessionStorage { get; } = new();
    }

    public sealed class StorageConfiguration
    {
        /// <summary>
        /// Initial data for the storage instance. When null, the storage starts empty.
        /// </summary>
        public IDictionary<string, string>? DataProvider { get; set; }

        /// <summary>
        /// Configurator that is called with the created <see cref="StorageInstance"/>.
        /// This can be used to subscribe to events.
        /// </summary>
        public Action<StorageInstance>? Configurator { get; set; }
    }
}
