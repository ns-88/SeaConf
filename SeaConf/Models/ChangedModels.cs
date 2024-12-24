using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SeaConf.Core;
using SeaConf.Infrastructure;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Core;

namespace SeaConf.Models
{
    using ModelProperties = (IMemoryModel Model, Dictionary<string, IPropertyData> Properties);

    /// <summary>
    /// Modified data models.
    /// </summary>
    internal class ChangedModels : IChangedModels, IReadOnlyList<IMemoryModel>, IReadOnlyDictionary<ModelData, IMemoryModel>
    {
        private readonly Dictionary<ModelData, ModelProperties> _models = new();

        /// <summary>
        /// Configuration has been changed.
        /// </summary>
        public bool HasChanged => _models.Count != 0;

        /// <summary>
        /// Attempt to get modified model properties.
        /// </summary>
        /// <typeparam name="T">Model type.</typeparam>
        /// <param name="modelName">Model name.</param>
        /// <param name="properties">Modified properties.</param>
        /// <returns>Sign of success.</returns>
        public bool TryGetProperties<T>(string modelName, [MaybeNullWhen(false)] out IReadOnlyCollection<IPropertyData> properties)
        {
            properties = null;

            var key = new ModelData(modelName, typeof(T));

            if (!HasChanged || !_models.TryGetValue(key, out var values))
            {
                return false;
            }

            properties = values.Properties.Values;

            return true;
        }

        /// <summary>
        /// Checking for property value change.
        /// </summary>
        /// <typeparam name="T">Model type.</typeparam>
        /// <param name="modelName">Model name.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Sign of change.</returns>
        public bool CheckProperty<T>(string modelName, string propertyName)
        {
            Guard.ThrowIfEmptyString(propertyName);

            var key = new ModelData(modelName, typeof(T));

            if (!HasChanged || !_models.TryGetValue(key, out var values))
            {
                return false;
            }

            return values.Properties.ContainsKey(propertyName);
        }

        /// <summary>
        /// Adding data about a changed property.
        /// </summary>
        /// <param name="propertyData">Property.</param>
        /// <param name="memoryModel">Model.</param>
        public void Add(IPropertyData propertyData, IMemoryModel memoryModel)
        {
            Guard.ThrowIfNull(propertyData);
            Guard.ThrowIfNull(memoryModel);

            var key = new ModelData(memoryModel.Name, memoryModel.Type);

            if (_models.TryGetValue(key, out var properties))
            {
                properties.Properties.Add(propertyData.Name, propertyData);
            }
            else
            {
                _models.Add(key, new ModelProperties(memoryModel, new Dictionary<string, IPropertyData> { { propertyData.Name, propertyData } }));
            }
        }

        #region Implementation of IEnumerable

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<KeyValuePair<ModelData, IMemoryModel>> IEnumerable<KeyValuePair<ModelData, IMemoryModel>>.GetEnumerator()
        {
            return _models.Select(model => new KeyValuePair<ModelData, IMemoryModel>(model.Key, model.Value.Model)).GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<IMemoryModel> IEnumerable<IMemoryModel>.GetEnumerator()
        {
            return _models.Select(model => model.Value.Model).GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _models.GetEnumerator();
        }

        #endregion

        #region Implementation of IReadOnlyCollection<out IMemoryModel>

        /// <summary>Gets the number of elements in the collection.</summary>
        /// <returns>The number of elements in the collection.</returns>
        int IReadOnlyCollection<IMemoryModel>.Count => _models.Count;

        #endregion

        #region Implementation of IReadOnlyList<out IMemoryModel>

        /// <summary>Gets the element at the specified index in the read-only list.</summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index in the read-only list.</returns>
        public IMemoryModel this[int index] => _models.ElementAt(index).Value.Model;

        #endregion

        #region Implementation of IReadOnlyCollection<out KeyValuePair<ModelData,IMemoryModel>>

        /// <summary>Gets the number of elements in the collection.</summary>
        /// <returns>The number of elements in the collection.</returns>
        int IReadOnlyCollection<KeyValuePair<ModelData, IMemoryModel>>.Count => _models.Count;

        #endregion

        #region Implementation of IReadOnlyDictionary<ModelData,IMemoryModel>

        /// <summary>Determines whether the read-only dictionary contains an element that has the specified key.</summary>
        /// <param name="key">The key to locate.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <returns>
        /// <see langword="true" /> if the read-only dictionary contains an element that has the specified key; otherwise, <see langword="false" />.</returns>
        public bool ContainsKey(ModelData key)
        {
            return _models.ContainsKey(key);
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
            value = null;

            if (!_models.TryGetValue(key, out var modelProperties))
            {
                return false;
            }

            value = modelProperties.Model;

            return true;
        }

        /// <summary>Gets the element that has the specified key in the read-only dictionary.</summary>
        /// <param name="key">The key to locate.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> is not found.</exception>
        /// <returns>The element that has the specified key in the read-only dictionary.</returns>
        public IMemoryModel this[ModelData key] => _models[key].Model;

        /// <summary>Gets an enumerable collection that contains the keys in the read-only dictionary.</summary>
        /// <returns>An enumerable collection that contains the keys in the read-only dictionary.</returns>
        public IEnumerable<ModelData> Keys => _models.Keys;

        /// <summary>Gets an enumerable collection that contains the values in the read-only dictionary.</summary>
        /// <returns>An enumerable collection that contains the values in the read-only dictionary.</returns>
        public IEnumerable<IMemoryModel> Values => _models.Values.Select(x => x.Model);

        #endregion
    }
}