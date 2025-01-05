using System;
using System.Threading.Tasks;
using SeaConf.Interfaces.Core;

namespace SeaConf.Models
{
    /// <summary>
    /// A composite configuration data model that combines an in memory and in storage data model.
    /// </summary>
	internal readonly struct CompositeModel : IAsyncDisposable
	{
        /// <summary>
        /// Data model in memory.
        /// </summary>
		public readonly IMemoryModel MemoryModel;

        /// <summary>
        /// Data model in storage.
        /// </summary>
		public readonly IStorageModel StorageModel;

		public CompositeModel(IMemoryModel memoryModel, IStorageModel storageModel)
		{
			MemoryModel = memoryModel;
			StorageModel = storageModel;
		}

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString()
        {
            return $"MemoryModel: {MemoryModel}, StorageModel: {StorageModel}";
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public ValueTask DisposeAsync()
        {
            return StorageModel?.DisposeAsync() ?? ValueTask.CompletedTask;
        }

        public void Deconstruct(out IMemoryModel memoryModel, out IStorageModel storageModel)
        {
            memoryModel = MemoryModel;
            storageModel = StorageModel;
        }
    }
}