using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SeaConf.Core.ValueProviders;
using SeaConf.Infrastructure;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Core;
using SeaConf.Models;

namespace SeaConf.Core
{
    /// <summary>
    /// Base configuration input/output.
    /// </summary>
    internal class ConfigurationIoBase
    {
        private readonly IMemorySource _memorySource;
        private readonly IStorageSource _storageSource;
        private readonly SyncMode _synchronizationMode;

        protected readonly IComponents Components;
        protected readonly ValueProvidersFactory ValueProvidersFactory;

        public ConfigurationIoBase(IMemorySource memorySource,
                                   IStorageSource storageSource,
                                   ValueProvidersFactory valueProvidersFactory,
                                   IComponents components,
                                   SyncMode synchronizationMode)
        {
            _memorySource = memorySource;
            _storageSource = storageSource;
            _synchronizationMode = synchronizationMode;

            ValueProvidersFactory = valueProvidersFactory;
            Components = components;
        }

        /// <summary>
        /// Getting all data models from storage and memory sources.
        /// </summary>
        /// <param name="synchronizedNodes">Synchronized nodes.</param>
        /// <returns>All data models from storage and memory sources.</returns>
        private async Task<Models> GetAllModelsFromSourcesAsync(IReadOnlyList<SynchronizedNodes> synchronizedNodes)
        {
            var memoryModels = _memorySource.GetModelsAsync(synchronizedNodes.Select(x => x.MemoryNode)).ConfigureAwait(false);
            var storageModels = _storageSource.GetModelsAsync(synchronizedNodes.Select(x => x.StorageNode)).ConfigureAwait(false);
            
            var memoryModelsMap = new Dictionary<ModelPath, IMemoryModel>();
            var storageModelsMap = new Dictionary<ModelPath, IStorageModel>();

            var memoryModelsMapFillTask = Task.Run(async () =>
            {
                await foreach (var memoryModel in memoryModels.ConfigureAwait(false))
                {
                    memoryModelsMap.Add(memoryModel.Path, memoryModel);
                }
            });

            var storageModelsMapFillTask = Task.Run(async () =>
            {
                await foreach (var storageModel in storageModels.ConfigureAwait(false))
                {
                    storageModelsMap.Add(storageModel.Path, storageModel);
                }
            });

            try
            {
                await Task.WhenAll(memoryModelsMapFillTask, storageModelsMapFillTask).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.GetModelsFromMemoryAndStorageFailed, ex);
            }

            return new Models(memoryModelsMap, storageModelsMap);
        }

        /// <summary>
        /// Synchronizing data models between in memory and storage source.
        /// </summary>
        /// <param name="rootMemoryNodes">Root nodes of source in memory.</param>
        /// <param name="rootStorageNodes">Root nodes of source in storage.</param>
        /// <returns>Synchronized data models.</returns>
        private async Task<IReadOnlyList<CompositeModel>> SynchronizationAsync(IReadOnlyCollection<INode> rootMemoryNodes, IReadOnlyCollection<INode> rootStorageNodes)
        {
            if (rootMemoryNodes.Count == 0)
            {
                throw new InvalidOperationException(Strings.ViolationStorageStructureNoModelsInStorage);
            }

            // ~~~Синхронизируем корневые модели~~~.

            var nodeFound = false;
            var synchronizedNodes = new List<SynchronizedNodes>();
            var synchronizedModels = new List<CompositeModel>();

            // Добавление моделей.
            foreach (var rootMemoryNode in rootMemoryNodes)
            {
                foreach (var rootStorageNode in rootStorageNodes)
                {
                    if (!rootMemoryNode.Name.Equals(rootStorageNode.Name, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    synchronizedNodes.Add(new SynchronizedNodes(rootMemoryNode, rootStorageNode));

                    nodeFound = true;
                    break;
                }

                if (nodeFound)
                {
                    nodeFound = false;
                    continue;
                }

                INode rootStorageNodeNew;
                var path = ((IModel)rootMemoryNode).Path;

                try
                {
                    rootStorageNodeNew = (INode)await _storageSource.AddModelAsync(path).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(string.Format(Strings.AddingNewRootModelFailed, path, rootMemoryNode.Name), ex);
                }

                synchronizedNodes.Add(new SynchronizedNodes(rootMemoryNode, rootStorageNodeNew));
            }

            nodeFound = false;

            // Удаление моделей.
            foreach (var rootStorageNode in rootStorageNodes)
            {
                foreach (var node in synchronizedNodes)
                {
                    if (!rootStorageNode.Name.Equals(node.StorageNode.Name, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    nodeFound = true;
                    break;
                }

                if (nodeFound)
                {
                    nodeFound = false;
                    continue;
                }

                var path = ((IModel)rootStorageNode).Path;

                try
                {
                    await _storageSource.DeleteModelAsync(path).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(string.Format(Strings.DeletingRootModelFailed, path, rootStorageNode.Name), ex);
                }
            }

            // ~~~Синхронизируем дочерние модели~~~.

            // Получаем все модели из источника в памяти и хранилище.
            var models = await GetAllModelsFromSourcesAsync(synchronizedNodes).ConfigureAwait(false);

            // Удаление моделей.
            foreach (var storageModel in models.Storage)
            {
                if (models.Memory.ContainsKey(storageModel.Key))
                {
                    continue;
                }

                try
                {
                    var nodes = await ((INode)storageModel.Value).GetDescendantNodesAsync().ConfigureAwait(false);

                    foreach (var node in nodes)
                    {
                        models.Storage.Remove(((IModel)node).Path);
                    }

                    models.Storage.Remove(storageModel.Key);

                    await _storageSource.DeleteModelAsync(storageModel.Key).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(string.Format(Strings.DeletingModelFailed, storageModel.Key, storageModel.Value.Name), ex);
                }
            }

            // Добавление моделей.
            foreach (var (path, memoryModel) in models.Memory)
            {
                if (!models.Storage.TryGetValue(path, out var storageModel))
                {
                    try
                    {
                        storageModel = await _storageSource.AddModelAsync(path).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(string.Format(Strings.AddingNewModelFailed, path, memoryModel.Name), ex);
                    }
                }

                synchronizedModels.Add(new CompositeModel(memoryModel, storageModel));
            }

            return synchronizedModels;
        }

        /// <summary>
        /// Getting composite models.
        /// </summary>
        /// <returns>Composite models.</returns>
        private async IAsyncEnumerable<CompositeModel> GetModelsAsync()
        {
            var rootMemoryNodes = await _memorySource.GetRootNodesAsync().ConfigureAwait(false);
            var rootStorageNodes = await _storageSource.GetRootNodesAsync().ConfigureAwait(false);

            if (_synchronizationMode == SyncMode.Enable || (_synchronizationMode == SyncMode.EnableIfDebug && Debugger.IsAttached))
            {
                IReadOnlyList<CompositeModel> compositeModels;

                try
                {
                    compositeModels = await SynchronizationAsync(rootMemoryNodes, rootStorageNodes).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(Strings.SynchronizingDataModelsFailed, ex);
                }

                foreach (var model in compositeModels)
                {
                    yield return model;
                }
            }
            else
            {
                if (rootMemoryNodes.Count == 0)
                {
                    throw new InvalidOperationException(Strings.ViolationStorageStructureNoModelsInMemory);
                }

                if (rootStorageNodes.Count == 0)
                {
                    throw new InvalidOperationException(Strings.ViolationStorageStructureNoModelsInStorage);
                }

                if (rootMemoryNodes.Count != rootStorageNodes.Count)
                {
                    throw new InvalidOperationException(string.Format(Strings.ViolationStorageStructureRootModelsNumberDoesNotMatch,
                        rootMemoryNodes.Count, rootStorageNodes.Count));
                }

                // ~~~Синхронизируем корневые модели, сопоставляя модели в памяти с моделями в источнике~~~.

                var nodeFound = false;
                var synchronizedNodes = new List<SynchronizedNodes>();

                foreach (var memoryNode in rootMemoryNodes)
                {
                    foreach (var storageNode in rootStorageNodes)
                    {
                        if (!memoryNode.Name.Equals(storageNode.Name, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        synchronizedNodes.Add(new SynchronizedNodes(memoryNode, storageNode));
                        nodeFound = true;

                        break;
                    }

                    if (!nodeFound)
                    {
                        throw new InvalidOperationException(string.Format(Strings.ViolationStorageStructureModelFromStorageNotFound,
                            ((IModel)memoryNode).Path, memoryNode.Name));
                    }

                    nodeFound = false;
                }

                // Получаем все модели из источника в памяти и хранилище.
                var models = await GetAllModelsFromSourcesAsync(synchronizedNodes).ConfigureAwait(false);

                if (models.Memory.Count != models.Storage.Count)
                {
                    throw new InvalidOperationException(string.Format(Strings.ViolationStorageStructureModelsNumberDoesNotMatch,
                        models.Memory.Count, models.Storage.Count));
                }

                foreach (var (path, memoryModel) in models.Memory)
                {
                    if (!models.Storage.TryGetValue(path, out var storageModel))
                    {
                        throw new InvalidOperationException(string.Format(Strings.ViolationStorageStructureModelFromStorageNotFound,
                            path, memoryModel.Name));
                    }

                    yield return new CompositeModel(memoryModel, storageModel);
                }
            }
        }

        /// <summary>
        /// Creating a composite source.
        /// </summary>
        /// <returns>Composite source.</returns>
        protected CompositeSource CreateCompositeSource()
        {
            return new CompositeSource(this);
        }

        #region Nested types

        /// <summary>
        /// Synchronized nodes.
        /// </summary>
        /// <param name="MemoryNode">Nodes from an in memory source.</param>
        /// <param name="StorageNode">Nodes from source in storage.</param>
        private readonly record struct SynchronizedNodes(INode MemoryNode, INode StorageNode);

        /// <summary>
        /// Data models from the source in memory and storage.
        /// </summary>
        /// <param name="Memory">Models from an in memory source.</param>
        /// <param name="Storage">Models from source in storage.</param>
        private readonly record struct Models(IReadOnlyDictionary<ModelPath, IMemoryModel> Memory, Dictionary<ModelPath, IStorageModel> Storage);

        /// <summary>
        /// Composite data model source.
        /// </summary>
        public readonly struct CompositeSource : IAsyncDisposable
        {
            private readonly ConfigurationIoBase _configurationIo;

            public CompositeSource(ConfigurationIoBase configurationIo)
            {
                _configurationIo = configurationIo;
            }

            /// <summary>
            /// Getting composite models.
            /// </summary>
            /// <returns>Composite models.</returns>
            public IAsyncEnumerable<CompositeModel> GetModelsAsync()
            {
                return _configurationIo.GetModelsAsync();
            }

            /// <summary>
            /// Loading.
            /// </summary>
            public async ValueTask LoadAsync()
            {
                await _configurationIo._storageSource.LoadAsync().ConfigureAwait(false);
            }

            /// <summary>
            /// Saving.
            /// </summary>
            public async ValueTask SaveAsync()
            {
                await _configurationIo._storageSource.SaveAsync().ConfigureAwait(false);
            }

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
            /// <returns>A task that represents the asynchronous dispose operation.</returns>
            public async ValueTask DisposeAsync()
            {
                if (_configurationIo._storageSource != null!)
                {
                    await _configurationIo._storageSource.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion
    }
}