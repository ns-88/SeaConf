using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SeaConf.Core;

namespace SeaConf.Interfaces.Core
{
    /// <summary>
    /// Configuration data source.
    /// </summary>
    /// <typeparam name="TModel">Model type.</typeparam>
    public interface ISource<out TModel>
		where TModel : class, IModel
	{
        /// <summary>
        /// Getting root configuration elements.
        /// </summary>
        /// <returns>Root configuration elements.</returns>
		ValueTask<IReadOnlyList<INode>> GetRootNodesAsync();

        /// <summary>
        /// Getting all data models from configuration.
        /// </summary>
        /// <param name="rootNodes">Root configuration elements.</param>
        /// <returns>All data models from configuration.</returns>
		IAsyncEnumerable<TModel> GetModelsAsync(IEnumerable<INode> rootNodes);
	}

    /// <summary>
    /// Configuration data source in storage.
    /// </summary>
    public interface IStorageSource : ISource<IStorageModel>, IAsyncDisposable
    {
        /// <summary>
        /// Loading.
        /// </summary>
        ValueTask LoadAsync();

        /// <summary>
        /// Saving.
        /// </summary>
        ValueTask SaveAsync();

        /// <summary>
        /// Adding a data model.
        /// </summary>
        /// <param name="path">Unique path.</param>
        /// <returns>Created data model.</returns>
        ValueTask<IStorageModel> AddModelAsync(ModelPath path);

        /// <summary>
        /// Deleting a data model.
        /// </summary>
        /// <param name="path">Unique path.</param>
        ValueTask DeleteModelAsync(ModelPath path);
    }

    /// <summary>
    /// Configuration data source in memory.
    /// </summary>
    public interface IMemorySource : ISource<IMemoryModel>
    {
        /// <summary>
        /// Configuration data models.
        /// </summary>
        IReadOnlyDictionary<ModelData, IMemoryModel> Models { get; }

        /// <summary>
        /// Getting data model.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="name">Name</param>
        /// <returns>Data model.</returns>
        T GetModel<T>(string? name) where T : class;

        /// <summary>
        /// Initializing.
        /// </summary>
        /// <param name="components">Configuration components.</param>
        void Initialize(IComponents components);
    }
}