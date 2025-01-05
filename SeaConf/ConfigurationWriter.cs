using System;
using System.Threading.Tasks;
using SeaConf.Core;
using SeaConf.Core.ValueProviders;
using SeaConf.Infrastructure;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Core;
using SeaConf.Models;

namespace SeaConf
{
    /// <summary>
    /// Configuration writer.
    /// </summary>
    internal class ConfigurationWriter : ConfigurationIoBase
    {
        public ConfigurationWriter(IMemorySource memorySource,
                                   IStorageSource storageSource,
                                   ValueProvidersFactory valueProvidersFactory,
                                   IComponents components,
                                   SyncMode synchronizationMode)
            : base(memorySource, storageSource, valueProvidersFactory, components, synchronizationMode)
        {
        }

        /// <summary>
        /// Writing.
        /// </summary>
        public async Task WriteAsync()
        {
            var changedModels = new ChangedModels();

            // Создание общего источника данных, объединяющего источники в хранилище и памяти.
            var compositeSource = CreateCompositeSource();

            try
            {
                // Загрузка данных из источников.
                await compositeSource.LoadAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.FailedLoadSettingsFromSource, ex);
            }

            // Получение итератора списка композитных моделей.
            var compositeModels = compositeSource.GetModelsAsync().ConfigureAwait(false);

            await foreach (var compositeModel in compositeModels.ConfigureAwait(false))
            {
                var (memoryModel, storageModel) = compositeModel;

                try
                {
                    // Создание средства записи данных в хранилище для текущей композитной модели.
                    var writer = storageModel.CreateWriter();

                    // Получение свойств, у которых были изменены данные из модели в памяти.
                    var properties = memoryModel.GetModifiedProperties();

                    foreach (var property in properties)
                    {
                        // Создание провайдера чтения и записи данных для конкретного типа.
                        if (!ValueProvidersFactory.TryCreate(property.Type, out var provider))
                        {
                            throw new InvalidOperationException(string.Format(Strings.ValueProviderNotFound, property.Type.Name));
                        }

                        try
                        {
                            // Запись значения в хранилище.
                            await provider.SetAsync(writer, property).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException(string.Format(Strings.FailedSavePropertyValue, property.Name, memoryModel.Name), ex);
                        }

                        changedModels.Add(property, memoryModel);
                    }

                    try
                    {
                        await storageModel.SaveAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(string.Format(Strings.ModelSaveFailed, storageModel.Name, storageModel.Path), ex);
                    }
                }
                finally
                {
                    await compositeModel.DisposeAsync().ConfigureAwait(false);
                }
            }

            try
            {
                // Сохранение данных в источник.
                await compositeSource.SaveAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.FailedSaveSettingsToSource, ex);
            }

            Components.RaiseSavedEvent(changedModels);
        }
    }
}