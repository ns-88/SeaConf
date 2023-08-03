using System;
using System.Diagnostics.CodeAnalysis;
using AppSettingsMini.Interfaces;

namespace AppSettingsMini.Models
{
    public class PropertyChangedEventArgs : EventArgs
    {
        private readonly Type _type;
        public readonly string Name;
        public readonly ISettingsModel Model;

        public PropertyChangedEventArgs(string name, ISettingsModel model)
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
                throw new InvalidOperationException();
            }

            var value = prop.GetValue(Model);

            if (value == null)
            {
                throw new InvalidOperationException();
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
                throw new InvalidCastException("");
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