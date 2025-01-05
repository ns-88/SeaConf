using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SeaConf.Core;
using SeaConf.Core.ValueProviders;
using SeaConf.Infrastructure;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Core;
using SeaConf.Interfaces.Factories;
using SeaConf.Models;

namespace SeaConf
{
    /// <summary>
    /// Configuration access tool - reading and writing data, subscribing to events, and receive data models.
    /// </summary>
    internal class Configuration : IConfiguration, IComponents, IReadOnlyList<IMemoryModel>, IReadOnlyDictionary<ModelData, IMemoryModel>
    {
        #region Configuration

        private readonly ISourceFactory _sourceFactory;
        private readonly IMemorySource _memorySource;
        private readonly ValueProvidersManager _valueProvidersManager;
        private readonly SyncMode _syncMode;

        /// <summary>
        /// Registered data models.
        /// </summary>
        public IReadOnlyDictionary<ModelData, IModel> RegisteredModels { get; }

        /// <summary>
        /// Known types.
        /// </summary>
        public IReadOnlyDictionary<Type, Type> KnownTypes { get; }

        /// <summary>
        /// Loading event.
        /// </summary>
        public event EventHandler? Loaded;

        /// <summary>
        /// Saving event.
        /// </summary>
        public event EventHandler<SavedEventArgs>? Saved;

        /// <summary>
        /// Property change event in data model.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs>? PropertyChanged;

        public Configuration(IReadOnlyDictionary<ModelData, IModel> registeredModels,
                             IReadOnlyDictionary<Type, Type> knownTypes,
                             ISourceFactory sourceFactory,
                             IReadOnlyDictionary<Type, IValueProviderFactory> valueProviderFactories,
                             SyncMode syncMode = SyncMode.Disable)
        {
            _memorySource = sourceFactory.CreateMemorySource();
            _valueProvidersManager = new ValueProvidersManager(valueProviderFactories);
            _sourceFactory = sourceFactory;
            _syncMode = syncMode;

            RegisteredModels = registeredModels;
            KnownTypes = knownTypes;

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                if (_memorySource == null!)
                {
                    throw new InvalidOperationException(Strings.MemorySourceNotCreated);
                }

                _memorySource.Initialize(this);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.MemorySourceInitializationFailed, ex);
            }
        }

        /// <summary>
        /// Loading.
        /// </summary>
        public async ValueTask LoadAsync()
        {
            try
            {
                // Создание фабрики провайдеров чтения и записи данных.
                using var valueProvidersFactory = _valueProvidersManager.CreateFactory();

                // Создание источника конфигурации в хранилище.
                var storageSource = _sourceFactory.CreateStorageSource();

                await using (storageSource.ConfigureAwait(false))
                {
                    // Создание средства чтения конфигурации.
                    var reader = new ConfigurationReader(_memorySource, storageSource, valueProvidersFactory, this, _syncMode);

                    // Выполнение чтения конфигурации.
                    await reader.ReadAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new ConfigurationLoadOrSaveFaultException(Strings.ConfigurationLoadFailed, ex);
            }
        }

        /// <summary>
        /// Saving.
        /// </summary>
        public async ValueTask SaveAsync()
        {
            try
            {
                // Создание фабрики провайдеров чтения и записи данных.
                using var valueProvidersFactory = _valueProvidersManager.CreateFactory();

                // Создание источника конфигурации в хранилище.
                var storageSource = _sourceFactory.CreateStorageSource();

                await using (storageSource.ConfigureAwait(false))
                {
                    // Создание средства записи конфигурации.
                    var writer = new ConfigurationWriter(_memorySource, storageSource, valueProvidersFactory, this, _syncMode);

                    // Выполнение записи конфигурации.
                    await writer.WriteAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new ConfigurationLoadOrSaveFaultException(Strings.ConfigurationSaveFailed, ex);
            }
        }

        /// <summary>
        /// Getting data model.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="name">Name</param>
        /// <returns>Data model.</returns>
        public T GetModel<T>(string? name = null) where T : class
        {
            return _memorySource.GetModel<T>(name);
        }

        #endregion

        #region Components

        /// <summary>
        /// Raise configuration load event.
        /// </summary>
        public void RaiseLoadedEvent()
        {
            Volatile.Read(ref Loaded)?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raise configuration saving event.
        /// </summary>
        /// <param name="changedModels">Modified data models.</param>
        public void RaiseSavedEvent(IChangedModels changedModels)
        {
            Volatile.Read(ref Saved)?.Invoke(this, new SavedEventArgs(changedModels));
        }

        /// <summary>
        /// Raise roperty change event in data model.
        /// </summary>
        public void RaisePropertyChangedEvent(IPropertyData propertyData)
        {
            Volatile.Read(ref PropertyChanged)?.Invoke(this, new PropertyChangedEventArgs(propertyData));
        }

        /// <summary>
        /// Get comparer for comparing supported data types.
        /// </summary>
        /// <typeparam name="T">Supported data type.</typeparam>
        /// <returns>Comparer.</returns>
        public IEqualityComparer<T> GetComparer<T>()
        {
            return _valueProvidersManager.GetComparer<T>();
        }

        /// <summary>
        /// Throw exception if type is not supported.
        /// </summary>
        /// <param name="type">Type.</param>
        public void ThrowIfNotSupportedType(Type type)
        {
            _valueProvidersManager.ThrowIfNotSupportedType(type);
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<KeyValuePair<ModelData, IMemoryModel>> IEnumerable<KeyValuePair<ModelData, IMemoryModel>>.GetEnumerator()
        {
            return _memorySource.Models.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<IMemoryModel> IEnumerable<IMemoryModel>.GetEnumerator()
        {
            return _memorySource.Models.Values.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _memorySource.Models.GetEnumerator();
        }

        #endregion

        #region Implementation of IReadOnlyCollection<out IMemoryModel>

        /// <summary>Gets the number of elements in the collection.</summary>
        /// <returns>The number of elements in the collection.</returns>
        int IReadOnlyCollection<IMemoryModel>.Count => _memorySource.Models.Count;

        #endregion

        #region Implementation of IReadOnlyList<out IMemoryModel>

        /// <summary>Gets the element at the specified index in the read-only list.</summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index in the read-only list.</returns>
        public IMemoryModel this[int index] => _memorySource.Models.Values.ElementAt(index);

        #endregion

        #region Implementation of IReadOnlyCollection<out KeyValuePair<string,IMemoryModel>>

        /// <summary>Gets the number of elements in the collection.</summary>
        /// <returns>The number of elements in the collection.</returns>
        int IReadOnlyCollection<KeyValuePair<ModelData, IMemoryModel>>.Count => _memorySource.Models.Count;

        #endregion

        #region Implementation of IReadOnlyDictionary<string,IMemoryModel>

        /// <summary>Determines whether the read-only dictionary contains an element that has the specified key.</summary>
        /// <param name="key">The key to locate.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <returns>
        /// <see langword="true" /> if the read-only dictionary contains an element that has the specified key; otherwise, <see langword="false" />.</returns>
        public bool ContainsKey(ModelData key)
        {
            return _memorySource.Models.ContainsKey(key);
        }

        /// <summary>Gets the value that is associated with the specified key.</summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <returns>
        /// <see langword="true" /> if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2" /> interface contains an element that has the specified key; otherwise, <see langword="false" />.</returns>
        public bool TryGetValue(ModelData key, [MaybeNullWhen(false)] out IMemoryModel value)
        {
            return _memorySource.Models.TryGetValue(key, out value);
        }

        /// <summary>Gets the element that has the specified key in the read-only dictionary.</summary>
        /// <param name="key">The key to locate.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> is not found.</exception>
        /// <returns>The element that has the specified key in the read-only dictionary.</returns>
        public IMemoryModel this[ModelData key] => _memorySource.Models[key];

        /// <summary>Gets an enumerable collection that contains the keys in the read-only dictionary.</summary>
        /// <returns>An enumerable collection that contains the keys in the read-only dictionary.</returns>
        public IEnumerable<ModelData> Keys => _memorySource.Models.Keys;

        /// <summary>Gets an enumerable collection that contains the values in the read-only dictionary.</summary>
        /// <returns>An enumerable collection that contains the values in the read-only dictionary.</returns>
        public IEnumerable<IMemoryModel> Values => _memorySource.Models.Values;

        #endregion
    }
}