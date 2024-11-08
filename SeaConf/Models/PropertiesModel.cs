using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SeaConf.Core;
using SeaConf.Infrastructure;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Core;

namespace SeaConf.Models
{
    using PropertiesData = IReadOnlyDictionary<string, IPropertyData>;

    /// <summary>
    /// Configuration data model in memory with support for reading and writing properties.
    /// </summary>
    public class PropertiesModel : ModelBase, IMemoryModel, IMemoryInitializedModel, IReadOnlyList<IPropertyData>, PropertiesData
    {
        private PropertiesData _storage;

        #region ElementsCount
        private ElementsCount _elementsCount;

        /// <summary>
        /// Number of elements.
        /// </summary>
        ElementsCount IMemoryModel.ElementsCount
        {
            get
            {
                ThrowIfNoInit();
                return _elementsCount;
            }
        }
        #endregion

        #region Name
        private string _name;

        /// <summary>
        /// Name.
        /// </summary>
        string IModel.Name
        {
            get
            {
                ThrowIfNoInit();
                return _name;
            }
        }

        #endregion

        #region Type
        private Type _type;

        /// <summary>
        /// Type.
        /// </summary>
        Type IMemoryModel.Type
        {
            get
            {
                ThrowIfNoInit();
                return _type;
            }
        }
        #endregion

        #region Path
        private ModelPath _path;

        /// <summary>
        /// Path.
        /// </summary>
        ModelPath IModel.Path
        {
            get
            {
                ThrowIfNoInit();
                return _path;
            }
        }
        #endregion

#nullable disable
        // ReSharper disable once NotNullMemberIsNotInitialized
        protected PropertiesModel()
        {
        }
#nullable restore

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="path">Path.</param>
        /// <param name="type">Type.</param>
        /// <param name="components">Components.</param>
        void IMemoryInitializedModel.Initialize(string name, ModelPath path, Type type, IComponents components)
        {
            _storage = CreatePropertiesData(type, this, components, out var elementsCount);
            _name = name;
            _path = path;
            _type = type;
            _elementsCount = elementsCount;
            
            SetInit();
        }

        /// <summary>
        /// Creating an internal data storage.
        /// </summary>
        /// <param name="modelType">Model type.</param>
        /// <param name="model">Model.</param>
        /// <param name="components">Configuration components.</param>
        /// <param name="elementsCount">Elements number in model.</param>
        /// <returns>Data storage.</returns>
        private static PropertiesData CreatePropertiesData(IReflect modelType, IMemoryModel model, IComponents components, out ElementsCount elementsCount)
        {
            var propertiesCount = 0;
            var modelsCount = 0;
            var propertiesData = new Dictionary<string, IPropertyData>();
            var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanRead)
                {
                    throw new InvalidOperationException(string.Format(Strings.PropertyIsNotReadable, property.Name));
                }

                if (property.GetCustomAttribute<ModelAttribute>() != null)
                {
                    modelsCount++;
                    continue;
                }

                var propertyData = PropertyData.Create(property.Name, property.PropertyType, model, components);

                propertiesCount++;

                propertiesData.Add(property.Name, propertyData);
            }

            elementsCount = new ElementsCount(propertiesCount, modelsCount);

            return propertiesData;
        }

        /// <summary>
        /// Getting modified properties.
        /// </summary>
        /// <returns>Modified properties.</returns>
        IEnumerable<IPropertyData> IMemoryModel.GetModifiedProperties()
        {
            ThrowIfNoInit();
            return _storage.Values.Where(x => x.IsModified);
        }

        /// <summary>
        /// Getting all properties.
        /// </summary>
        /// <returns>All properties.</returns>
        IEnumerable<IPropertyData> IMemoryModel.GetProperties()
        {
            ThrowIfNoInit();
            return _storage.Values;
        }

        /// <summary>
        /// Setting property value.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="value">Value.</param>
        /// <param name="propertyName">Property name.</param>
        protected void SetValue<T>(T value, [CallerMemberName] string propertyName = "")
        {
            Guard.ThrowIfEmptyString(propertyName);
            ThrowIfNoInit();

            try
            {
                if (_storage.TryGetValue(propertyName, out var rawValue))
                {
                    rawValue.ToTyped<T>().Set(value);
                }
                else
                {
                    throw new KeyNotFoundException(string.Format(Strings.PropertyNotFound, propertyName));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format(Strings.FailedSetPropertyValue, propertyName, _name), ex);
            }
        }

        /// <summary>
        /// Getting property value.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value.</returns>
        protected T GetValue<T>([CallerMemberName] string propertyName = "")
        {
            Guard.ThrowIfEmptyString(propertyName);
            ThrowIfNoInit();

            try
            {
                if (_storage.TryGetValue(propertyName, out var rawValue))
                {
                    return rawValue.ToTyped<T>().Get();
                }

                throw new KeyNotFoundException(string.Format(Strings.PropertyNotFound, propertyName));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format(Strings.FailedGetPropertyValue, propertyName, _name), ex);
            }
        }

        #region Implementation of IEnumerable

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<KeyValuePair<string, IPropertyData>> IEnumerable<KeyValuePair<string, IPropertyData>>.GetEnumerator()
        {
            ThrowIfNoInit();
            return _storage.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<IPropertyData> IEnumerable<IPropertyData>.GetEnumerator()
        {
            ThrowIfNoInit();
            return _storage.Values.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowIfNoInit();
            return _storage.GetEnumerator();
        }

        #endregion

        #region Implementation of IReadOnlyCollection<out IPropertyData>

        /// <summary>Gets the number of elements in the collection.</summary>
        /// <returns>The number of elements in the collection.</returns>
        int IReadOnlyCollection<IPropertyData>.Count
        {
            get
            {
                ThrowIfNoInit();
                return _storage.Count;
            }
        }

        #endregion

        #region Implementation of IReadOnlyList<out IPropertyData>

        /// <summary>Gets the element at the specified index in the read-only list.</summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index in the read-only list.</returns>
        IPropertyData IReadOnlyList<IPropertyData>.this[int index]
        {
            get
            {
                ThrowIfNoInit();
                return _storage.Values.ElementAt(index);
            }
        }

        #endregion

        #region Implementation of IReadOnlyCollection<out KeyValuePair<string,IPropertyData>>

        /// <summary>Gets the number of elements in the collection.</summary>
        /// <returns>The number of elements in the collection.</returns>
        int IReadOnlyCollection<KeyValuePair<string, IPropertyData>>.Count
        {
            get
            {
                ThrowIfNoInit();
                return _storage.Count;
            }
        }

        #endregion

        #region Implementation of IReadOnlyDictionary<string,IPropertyData>

        /// <summary>Determines whether the read-only dictionary contains an element that has the specified key.</summary>
        /// <param name="key">The key to locate.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <returns>
        /// <see langword="true" /> if the read-only dictionary contains an element that has the specified key; otherwise, <see langword="false" />.</returns>
        bool PropertiesData.ContainsKey(string key)
        {
            ThrowIfNoInit();
            return _storage.ContainsKey(key);
        }

        /// <summary>Gets the value that is associated with the specified key.</summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <returns>
        /// <see langword="true" /> if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2" /> interface contains an element that has the specified key; otherwise, <see langword="false" />.</returns>
        bool PropertiesData.TryGetValue(string key, [MaybeNullWhen(false)] out IPropertyData value)
        {
            ThrowIfNoInit();
            return _storage.TryGetValue(key, out value);
        }

        /// <summary>Gets the element that has the specified key in the read-only dictionary.</summary>
        /// <param name="key">The key to locate.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> is not found.</exception>
        /// <returns>The element that has the specified key in the read-only dictionary.</returns>
        IPropertyData PropertiesData.this[string key]
        {
            get
            {
                ThrowIfNoInit();
                return _storage[key];
            }
        }

        /// <summary>Gets an enumerable collection that contains the keys in the read-only dictionary.</summary>
        /// <returns>An enumerable collection that contains the keys in the read-only dictionary.</returns>
        IEnumerable<string> PropertiesData.Keys
        {
            get
            {
                ThrowIfNoInit();
                return _storage.Keys;
            }
        }

        /// <summary>Gets an enumerable collection that contains the values in the read-only dictionary.</summary>
        /// <returns>An enumerable collection that contains the values in the read-only dictionary.</returns>
        IEnumerable<IPropertyData> PropertiesData.Values
        {
            get
            {
                ThrowIfNoInit();
                return _storage.Values;
            }
        }

        #endregion
    }
}