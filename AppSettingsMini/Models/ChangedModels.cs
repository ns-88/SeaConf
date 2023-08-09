using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AppSettingsMini.Interfaces;

namespace AppSettingsMini.Models
{
    internal class ChangedModels : IChangedModels
    {
        private readonly Dictionary<Type, HashSet<string>> _models;
        public bool HasChanged => _models.Count != 0;

        public ChangedModels()
        {
            _models = new Dictionary<Type, HashSet<string>>();
        }

        public bool TryGetProperties<T>([MaybeNullWhen(false)] out IReadOnlyCollection<string> properties)
        {
            properties = null;

            if (!HasChanged || !_models.TryGetValue(typeof(T), out var values))
            {
                return false;
            }

            properties = values;

            return true;
        }

        public bool CheckProperty<T>(string propertyName)
        {
            if (!HasChanged || !_models.TryGetValue(typeof(T), out var values))
            {
                return false;
            }

            return values.Contains(propertyName);
        }

        public void Add(Type type, string propertyName)
        {
            if (_models.TryGetValue(type, out var properties))
            {
                properties.Add(propertyName);
            }
            else
            {
                _models.Add(type, new HashSet<string> { propertyName });
            }
        }
    }
}