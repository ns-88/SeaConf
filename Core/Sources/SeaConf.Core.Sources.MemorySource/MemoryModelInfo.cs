using SeaConf.Common;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using SeaConf.Common.Attributes;
using SeaConf.Core.Common.Properties;

namespace SeaConf.Core.Sources.MemorySource;

internal sealed class MemoryModelInfo : INode, IMemoryModel
{
	private readonly List<INode> _innerModels;
	private readonly IReadOnlyDictionary<Type, Type> _knownTypes;
	private readonly IComponents _components;
	private readonly Dictionary<string, IPropertyData> _propertiesData;

	public IReadOnlyDictionary<string, IPropertyData> PropertiesData => _propertiesData;
	public required string Name { get; init; }
	public required IMemoryModel Model { get; init; }
	public required Type Type { get; init; }
	public ElementsCount ElementsCount { get; private set; }
	public bool IsInitialized { get; private set; }
	public required ModelPath Path { get; init; }

	public MemoryModelInfo(IComponents components)
	{
		_components = components;
		_knownTypes = components.KnownTypes;
		_innerModels = new List<INode>();
		_propertiesData = new Dictionary<string, IPropertyData>();
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
				throw new InvalidOperationException(string.Format(Resources.FailedCreateNestedModelInstance, IMemoryModel.GetName(property.PropertyType)), ex);
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
						throw new InvalidOperationException(string.Format(Resources.FailedGetBackingField, backingFieldName, property.Name));
					}
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(string.Format(Resources.FailedSetPropertyValue, property.Name, Name), ex);
			}

			innerModelRaw = innerModel;
		}

		if (innerModelRaw is not IMemoryModel innerMemoryModel)
		{
			throw new InvalidOperationException(string.Format(Resources.InvalidNestedModelType,
				typeof(ModelAttribute), typeof(IMemoryModel), property.PropertyType, Type));
		}

		var name = IMemoryModel.GetName(property.PropertyType, attribute);

		node = new MemoryModelInfo(_components)
		{
			Model = innerMemoryModel,
			Name = name,
			Type = property.PropertyType,
			Path = new ModelPath(name, Path)
		};

		return true;
	}

	/// <inheritdoc />
	public ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
	{
		if (IsInitialized)
		{
			return ValueTask.FromResult((IReadOnlyList<INode>)_innerModels);
		}

		var properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

		foreach (var property in properties)
		{
			if (!property.CanRead)
			{
				throw new InvalidOperationException(string.Format(Resources.PropertyIsNotReadable, property.Name));
			}

			if (TryCreatePropertiesNode(property, out var node))
			{
				_innerModels.Add(node);
			}
			else
			{
				var propertyData = PropertyData.Create(property.Name, property.PropertyType, Model, _components);

				_propertiesData.Add(property.Name, propertyData);
			}
		}

		IsInitialized = true;
		ElementsCount = new ElementsCount(_propertiesData.Count, _innerModels.Count);

		return ValueTask.FromResult((IReadOnlyList<INode>)_innerModels);
	}

	/// <inheritdoc />
	public IEnumerable<IPropertyData> GetModifiedProperties()
	{
		return Model.GetModifiedProperties();
	}

	/// <inheritdoc />
	public IEnumerable<IPropertyData> GetProperties()
	{
		return Model.GetProperties();
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"Name = {Name}, Type = {Type.Name}, InnerModels = {_innerModels.Count}";
	}
}