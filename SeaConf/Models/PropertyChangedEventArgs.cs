using System;
using System.Diagnostics.CodeAnalysis;
using SeaConf.Infrastructure;
using SeaConf.Interfaces.Core;

namespace SeaConf.Models
{
    /// <summary>
    /// Property change event data.
    /// </summary>
	public class PropertyChangedEventArgs : EventArgs
    {
        private readonly IPropertyData _propertyData;

        /// <summary>
        /// Name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Model.
        /// </summary>
        public readonly IMemoryModel Model;

        public PropertyChangedEventArgs(IPropertyData propertyData)
        {
            _propertyData = Guard.ThrowIfNull(propertyData);

            Name = _propertyData.Name;
            Model = _propertyData.Model;
        }

        /// <summary>
        /// Getting the property value.
        /// </summary>
        /// <returns>Property value.</returns>
        public object GetValue()
        {
            return _propertyData.Get<object>();
        }

        /// <summary>
        /// Getting a typed property value.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <returns>Typed property value.</returns>
        public T GetValue<T>()
        {
            return _propertyData.Get<T>();
        }

        /// <summary>
        /// An attempt to get a property value for a model of the specified type.
        /// </summary>
        /// <typeparam name="TModel">Model type.</typeparam>
        /// <param name="modelName">Model name.</param>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Sign of success.</returns>
        public bool TryGetValue<TModel>(string modelName, [MaybeNullWhen(false)] out object value, string? propertyName = null)
        {
            var localPropertyName = propertyName ?? Name;

            value = null;

            if (!Model.Name.Equals(modelName, StringComparison.Ordinal) || Model is not TModel || !Name.Equals(localPropertyName, StringComparison.Ordinal))
            {
                return false;
            }

            value = _propertyData.Get<object>();

            return true;
        }

        /// <summary>
        /// An attempt to get a property value for the model and a property of the specified type.
        /// </summary>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <typeparam name="TModel">Model type.</typeparam>
        /// <param name="modelName">Model name.</param>
        /// <param name="value">Typed property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Sign of success.</returns>
        public bool TryGetValue<TProperty, TModel>(string modelName, [MaybeNullWhen(false)] out TProperty value, string? propertyName = null)
        {
            value = default;

            if (!TryGetValue<TModel>(modelName, out var rawValue, propertyName) || rawValue is not TProperty property)
            {
                return false;
            }

            value = property;

            return true;
        }
    }
}