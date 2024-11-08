using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using SeaConf.Infrastructure;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Core;
using SeaConf.Models;

namespace SeaConf.Core.Sources
{
    /// <summary>
    /// Configuration data source in memory.
    /// </summary>
    internal class MemorySource : SourceBase<IMemoryModel>, IMemorySource
    {
        private IReadOnlyList<INode>? _rootNodes;

        /// <summary>
        /// Configuration data models.
        /// </summary>
        public IReadOnlyDictionary<ModelData, IMemoryModel> Models { get; }

        public MemorySource()
        {
            Models = new Dictionary<ModelData, IMemoryModel>();
        }

        /// <summary>
        /// Getting data model.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="name">Name</param>
        /// <returns>Data model.</returns>
        public T GetModel<T>(string? name) where T : class
        {
            if (name != null && name.Trim().Length == 0)
            {
                throw new ArgumentException(nameof(name));
            }

            var modelType = typeof(T);
            var modelName = name ?? IMemoryModel.GetName(modelType);

            if (!Models.TryGetValue(new ModelData(modelName, modelType), out var model))
            {
                throw new InvalidOperationException(string.Format(Strings.ModelNotRegistered, modelType, modelName));
            }

            return (T)model;
        }

        /// <summary>
        /// Initializing.
        /// </summary>
        /// <param name="components">Configuration components.</param>
        public void Initialize(IComponents components)
        {
            var source = new MemoryModelInfoSource(components.RegisteredModels, components.KnownTypes);
            var rootNodes = source.GetRootNodesAsync().Result;
            var infoModels = source.GetModelsAsync(rootNodes).ToBlockingEnumerable();
            var models = (Dictionary<ModelData, IMemoryModel>)Models;

            foreach (var modelInfo in infoModels)
            {
                var key = new ModelData(modelInfo.Name, modelInfo.Type);

                if (Models.ContainsKey(key))
                {
                    throw new InvalidOperationException(string.Format(Strings.ModelAlreadyAdded, key.Name, key.Type));
                }

                try
                {
                    ((IMemoryInitializedModel)modelInfo.Model).Initialize(modelInfo.Name, modelInfo.Path, modelInfo.Type, components);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(string.Format(Strings.ModelInitializationFailed, modelInfo.Type.Name), ex);
                }

                models.Add(key, modelInfo.Model);
            }

            _rootNodes = rootNodes;
        }

        /// <summary>
        /// Getting root configuration elements.
        /// </summary>
        /// <returns>Root configuration elements.</returns>
        public override ValueTask<IReadOnlyList<INode>> GetRootNodesAsync()
        {
            Guard.ThrowIfNull(_rootNodes);
            return ValueTask.FromResult(_rootNodes!);
        }

        #region Nested types

        private class MemoryModelInfoSource : SourceBase<MemoryModelInfo>
        {
            private readonly IReadOnlyDictionary<ModelData, IModel> _registeredModels;
            private readonly IReadOnlyDictionary<Type, Type> _knownTypes;

            public MemoryModelInfoSource(IReadOnlyDictionary<ModelData, IModel> registeredModels, IReadOnlyDictionary<Type, Type> knownTypes)
            {
                _registeredModels = registeredModels;
                _knownTypes = knownTypes;
            }

            public override ValueTask<IReadOnlyList<INode>> GetRootNodesAsync()
            {
                var nodes = new List<INode>();

                foreach (var (modelData, model) in _registeredModels)
                {
                    nodes.Add(new MemoryModelInfo(_knownTypes)
                    {
                        Model = (IMemoryModel)model,
                        Name = modelData.Name,
                        Type = modelData.Type,
                        Path = new ModelPath(modelData.Name)
                    });
                }

                return ValueTask.FromResult((IReadOnlyList<INode>)nodes);
            }
        }

        private class MemoryModelInfo : INode, IMemoryModel
        {
            private bool _isInitialized;
            private readonly List<INode> _innerModels;
            private readonly IReadOnlyDictionary<Type, Type> _knownTypes;

            public required string Name { get; init; }
            public required IMemoryModel Model { get; init; }
            public required Type Type { get; init; }
            public ElementsCount ElementsCount => Model.ElementsCount;
            public required ModelPath Path { get; init; }

            public MemoryModelInfo(IReadOnlyDictionary<Type, Type> knownTypes)
            {
                _innerModels = new List<INode>();
                _knownTypes = knownTypes;
            }

            private bool TryCreatePropertiesNode(PropertyInfo property, [MaybeNullWhen(false)] out INode node)
            {
                var attribute = property.GetCustomAttribute<ModelAttribute>();

                node = null;

                if (attribute == null)
                {
                    return false;
                }

                var innerModelRaw = property.GetValue(Model);

                if (innerModelRaw == null)
                {
                    object? innerModel;

                    if (!_knownTypes.TryGetValue(property.PropertyType, out var activatedType))
                    {
                        activatedType = property.PropertyType;
                    }

                    try
                    {
                        innerModel = Activator.CreateInstance(activatedType);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(string.Format(Strings.FailedCreateNestedModelInstance, IMemoryModel.GetName(property.PropertyType)), ex);
                    }

                    try
                    {
                        if (property.CanWrite)
                        {
                            property.SetValue(Model, innerModel);
                        }
                        else
                        {
                            var backingFieldName = $"<{property.Name}>k__BackingField";
                            var backingField = Model.GetType().GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

                            if (backingField != null)
                            {
                                backingField.SetValue(Model, innerModel);
                            }
                            else
                            {
                                throw new InvalidOperationException(string.Format(Strings.FailedGetBackingField, backingFieldName, property.Name));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(string.Format(Strings.FailedSetPropertyValue, property.Name, Name), ex);
                    }

                    innerModelRaw = innerModel;
                }

                if (innerModelRaw is not IMemoryModel innerMemoryModel)
                {
                    throw new InvalidOperationException(string.Format(Strings.InvalidNestedModelType,
                        typeof(ModelAttribute), typeof(IMemoryModel), property.PropertyType, Type));
                }

                var name = IMemoryModel.GetName(property.PropertyType, attribute);

                node = new MemoryModelInfo(_knownTypes)
                {
                    Model = innerMemoryModel,
                    Name = name,
                    Type = property.PropertyType,
                    Path = new ModelPath(name, Path)
                };

                return true;
            }

            public ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
            {
                if (_isInitialized)
                {
                    return ValueTask.FromResult((IReadOnlyList<INode>)_innerModels);
                }

                var nodes = new List<INode>();
                var properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    if (!TryCreatePropertiesNode(property, out var node))
                    {
                        continue;
                    }

                    _innerModels.Add(node);

                    nodes.Add(node);
                }

                _isInitialized = true;

                return ValueTask.FromResult((IReadOnlyList<INode>)nodes);
            }

            public IEnumerable<IPropertyData> GetModifiedProperties()
            {
                return Model.GetModifiedProperties();
            }

            public IEnumerable<IPropertyData> GetProperties()
            {
                return Model.GetProperties();
            }

            public override string ToString()
            {
                return $"Name = {Name}, Type = {Type.Name}, InnerModels = {_innerModels.Count}";
            }
        }

        #endregion
    }
}