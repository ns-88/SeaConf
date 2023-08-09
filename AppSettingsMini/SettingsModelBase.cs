using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;

namespace AppSettingsMini
{
	using PropertiesData = IReadOnlyDictionary<string, ISettingsPropertyData>;

	public class SettingsModelBase : ISettingsModel
	{
		private PropertiesData _storage;

#nullable disable
		// ReSharper disable once NotNullMemberIsNotInitialized
		protected SettingsModelBase()
		{
		}
#nullable restore

		void ISettingsModel.Init(SettingsServiceBase service)
		{
			_storage = CreatePropertiesData(GetType(), this, service);
		}

		IEnumerable<ISettingsPropertyData> ISettingsModel.GetModifiedProperties()
		{
			return _storage.Values.Where(x => x.IsModified);
		}

		PropertiesData ISettingsModel.GetPropertiesData()
		{
			return _storage;
		}

		private static PropertiesData CreatePropertiesData(IReflect modelType, ISettingsModel model, SettingsServiceBase service)
		{
			var propertiesData = new Dictionary<string, ISettingsPropertyData>();
			var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var property in properties)
			{
				if (!property.CanRead)
				{
					throw new InvalidOperationException();
				}

				var openGenericType = typeof(SettingsPropertyData<>);
				var typeArgs = new[] { property.PropertyType };
				var genericType = openGenericType.MakeGenericType(typeArgs);

				var ctor = genericType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
					null,
					CallingConventions.Standard | CallingConventions.HasThis,
					new[] { typeof(string), typeof(Type), typeof(ISettingsModel), typeof(SettingsServiceBase) },
					null);

				if (ctor == null)
				{
					throw new InvalidOperationException("");
				}

				var propertyData = (ISettingsPropertyData)ctor.Invoke(new object[] { property.Name, property.PropertyType, model, service });

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
					rawValue.ToTyped<T>().Set(value);
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
					return rawValue.ToTyped<T>().Get();
				}

				throw new KeyNotFoundException(string.Format(Strings.PropertyNotFound, propertyName));
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(string.Format(Strings.FailedGetPropertyValue, propertyName), ex);
			}
		}
	}
}