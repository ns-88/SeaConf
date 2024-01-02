using System;
using System.Threading.Tasks;
using AppSettingsMini.Interfaces.Core;

namespace AppSettingsMini.Models
{
	internal readonly struct CompositeModel : IAsyncDisposable
	{
		public readonly IMemoryModel MemoryModel;
		public readonly IStorageModel StorageModel;

		public CompositeModel(IMemoryModel memoryModel, IStorageModel storageModel)
		{
			MemoryModel = memoryModel;
			StorageModel = storageModel;
		}

		public override string ToString()
		{
			return $"MemoryModel = {MemoryModel}, StorageModel = {StorageModel}";
		}

		public ValueTask DisposeAsync()
		{
			return StorageModel?.DisposeAsync() ?? ValueTask.CompletedTask;
		}
	}
}