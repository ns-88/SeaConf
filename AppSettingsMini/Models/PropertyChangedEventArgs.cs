using System;
using System.Diagnostics.CodeAnalysis;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces.Core;

namespace AppSettingsMini.Models
{
	public class PropertyChangedEventArgs : EventArgs
    {
        private readonly Type _type;
        public readonly string Name;
        public readonly IMemoryModel Model;

        public PropertyChangedEventArgs(string name, IMemoryModel model)
        {
            _type = model.GetType();

            Name = name;
            Model = model;
        }

        private object GetValueInternal(string propertyName)
        {
            var prop = _type.GetProperty(propertyName);

            if (prop == null)
            {
                throw new InvalidOperationException(string.Format(Strings.PropertyNotFoundInType, propertyName, _type.Name));
            }

            var value = prop.GetValue(Model);

            if (value == null)
            {
                throw new InvalidOperationException(string.Format(Strings.PropertyValueNotSetInType, propertyName, _type.Name));
            }

            return value;
        }

        public object GetValue()
        {
            return GetValueInternal(Name);
        }

        public T GetValue<T>()
        {
            var rawValue = GetValue();

            if (rawValue is not T value)
            {
                throw new InvalidCastException(string.Format(Strings.FailedToCastTypeValue, rawValue.GetType().Name, typeof(T).Name));
            }

            return value;
        }

        public bool TryGetValue<TModel>([MaybeNullWhen(false)] out object value, string? propertyName = null)
        {
            var localPropertyName = propertyName ?? Name;

            value = null;

            if (Model is not TModel || !Name.Equals(localPropertyName, StringComparison.Ordinal))
            {
                return false;
            }

            value = GetValueInternal(localPropertyName);

            return true;
        }

        public bool TryGetValue<TProperty, TModel>([MaybeNullWhen(false)] out TProperty value, string? propertyName = null)
        {
            value = default;

            if (!TryGetValue<TModel>(out var rawValue, propertyName) || rawValue is not TProperty property)
            {
                return false;
            }

            value = property;

            return true;
        }
    }
}