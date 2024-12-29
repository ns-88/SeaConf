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
    /// Configuration reader.
    /// </summary>
    internal class ConfigurationReader : ConfigurationIoBase
    {
        public ConfigurationReader(IMemorySource memorySource,
                                   IStorageSource storageSource,
                                   ValueProvidersFactory valueProvidersFactory,
                                   IComponents components,
                                   SyncMode synchronizationMode)
            : base(memorySource, storageSource, valueProvidersFactory, components, synchronizationMode)
        {
        }

        /// <summary>
        /// Reading.
        /// </summary>
        public async Task ReadAsync()
        {
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
                    try
                    {
                        await storageModel.LoadAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(string.Format(Strings.ModelLoadFailed, storageModel.Name, storageModel.Path), ex);
                    }

                    // Создание средства чтения данных из хранилища для текущей композитной модели.
                    var reader = storageModel.CreateReader();

                    // Получение свойств из модели в памяти.
                    var properties = memoryModel.GetProperties();

                    foreach (var property in properties)
                    {
                        // Создание провайдера чтения и записи данных для конкретного типа.
                        if (!ValueProvidersFactory.TryCreate(property.Type, out var provider))
                        {
                            throw new InvalidOperationException(string.Format(Strings.ValueProviderNotFound, property.Type.Name));
                        }

                        IPropertyData value;

                        try
                        {
                            // Получение значения для конкретного свойства из хранилища.
                            value = await provider.GetAsync(reader, property).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException(string.Format(Strings.FailedLoadPropertyValue, property.Name, memoryModel.Name), ex);
                        }

                        try
                        {
                            // Присвоение полученного значения свойству.
                            property.Set(value);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException(string.Format(Strings.FailedSetPropertyValue, property.Name, memoryModel.Name), ex);
                        }
                    }
                }
                finally
                {
                    await compositeModel.DisposeAsync().ConfigureAwait(false);
                }
            }

            Components.RaiseLoadedEvent();
        }
    }
}