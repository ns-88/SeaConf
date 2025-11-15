using System.Diagnostics;
using SeaConf.Common;
using SeaConf.Core.Common.Abstractions.Models;
using System.Runtime.CompilerServices;
using SeaConf.Common.Infrastructure;

namespace SeaConf.Core.Common.Models;

/// <summary>
/// Configuration data model in memory with support for reading and writing properties.
/// </summary>
[DebuggerTypeProxy(typeof(PropertiesModelDebugView))]
public partial class PropertiesModel : ModelBase
{
	private IReadOnlyDictionary<string, IPropertyData> _propertiesData;

#nullable disable
	// ReSharper disable once NotNullMemberIsNotInitialized
	protected PropertiesModel()
	{
	}
#nullable restore

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
			if (_propertiesData.TryGetValue(propertyName, out var rawValue))
			{
				rawValue.ToTyped<T>().Set(value);
			}
			else
			{
				throw new KeyNotFoundException(string.Format(Resources.PropertyNotFound, propertyName));
			}
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(string.Format(Resources.FailedSetPropertyValue, propertyName, _name), ex);
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
			if (_propertiesData.TryGetValue(propertyName, out var rawValue))
			{
				return rawValue.ToTyped<T>().Get();
			}

			throw new KeyNotFoundException(string.Format(Resources.PropertyNotFound, propertyName));
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(string.Format(Resources.FailedGetPropertyValue, propertyName, _name), ex);
		}
	}

	#region Nested types

	private class PropertiesModelDebugView
	{
		public MemoryModelDebug Info { get; }
		public IReadOnlyList<IPropertyData> Properties { get; }

		public PropertiesModelDebugView(PropertiesModel propertiesModel)
		{
			Info = new MemoryModelDebug(propertiesModel);
			Properties = propertiesModel._propertiesData.Values.ToList();
		}

		[DebuggerDisplay("Name: {Name}")]
		internal class MemoryModelDebug
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly PropertiesModel _propertiesModel;

			public MemoryModelDebug(PropertiesModel propertiesModel)
			{
				_propertiesModel = propertiesModel;
			}

			public string Name => _propertiesModel._name;

			public ModelPath Path => _propertiesModel._path;

			public Type Type => _propertiesModel._type;

			public bool IsInitialized => _propertiesModel.IsInitialized;

			public ElementsCount ElementsCount => _propertiesModel._elementsCount;
		}
	}

	#endregion
}