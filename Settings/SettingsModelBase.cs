using System.Reflection;
using System.Runtime.CompilerServices;
using Settings.Infrastructure;
using Settings.Interfaces;

namespace Settings
{
	using PropertiesData = IReadOnlyDictionary<string, IPropertyData>;

	public class SettingsModelBase : ISettingsModel
	{
		private readonly PropertiesData _storage;

		protected SettingsModelBase()
		{
			_storage = CreatePropertiesData(GetType());
		}

		private static PropertiesData CreatePropertiesData(Type modelType)
		{
			var propertiesData = new Dictionary<string, IPropertyData>();
			var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var property in properties)
			{
				if (!property.CanRead)
				{
					throw new InvalidOperationException();
				}

				var openGenericType = typeof(PropertyData<>);
				var typeArgs = new[] { property.PropertyType };
				var genericType = openGenericType.MakeGenericType(typeArgs);

				var propertyData = (IPropertyData)Activator.CreateInstance(genericType, $"{modelType.Name}.{property.Name}", property.PropertyType)!;

				propertiesData.Add(property.Name, propertyData);
			}

			return propertiesData;
		}

		protected void SetValue<T>(T value, [CallerMemberName] string propertyName = "")
		{
			Guard.ThrowIfEmptyString(propertyName);

			try
			{
				if (_storage.TryGetValue(propertyName, out var rawValue))
				{
					IPropertyData.GetTyped<T>(rawValue).Set(value);
				}
				else
				{
					throw new KeyNotFoundException(string.Format(Strings.PropertyNotFound, propertyName));
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(string.Format(Strings.FailedSetPropertyValue, propertyName), ex);
			}
		}

		protected T GetValue<T>([CallerMemberName] string propertyName = "")
		{
			Guard.ThrowIfEmptyString(propertyName);

			try
			{
				if (_storage.TryGetValue(propertyName, out var rawValue))
				{
					return IPropertyData.GetTyped<T>(rawValue).Get();
				}

				throw new KeyNotFoundException(string.Format(Strings.PropertyNotFound, propertyName));
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(string.Format(Strings.FailedGetPropertyValue, propertyName), ex);
			}
		}

		IEnumerable<IPropertyData> ISettingsModel.GetModifiedProperties()
		{
			return _storage.Values.Where(x => x.IsModified);
		}

		PropertiesData ISettingsModel.GetPropertiesData()
		{
			return _storage;
		}
	}
}