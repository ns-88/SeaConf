using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AppSettingsMini.Factories;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;

namespace AppSettingsMini
{
	using PropertiesData = IReadOnlyDictionary<string, ISettingsPropertyData>;

	public class SettingsModelBase : ISettingsModel
	{
		private PropertiesData _storage;
		private bool _isInit;

#nullable disable
		// ReSharper disable once NotNullMemberIsNotInitialized
		protected SettingsModelBase()
		{
		}
#nullable restore

		void ISettingsModel.Init(SettingsServiceBase service)
		{
			_storage = CreatePropertiesData(GetType(), this, service);
			_isInit = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ThrowIfNoInit()
		{
			if (!_isInit)
			{
				throw new InvalidOperationException(string.Format(Strings.ModelNotInitialized, GetType().Name));
			}
		}

		IEnumerable<ISettingsPropertyData> ISettingsModel.GetModifiedProperties()
		{
			ThrowIfNoInit();

			return _storage.Values.Where(x => x.IsModified);
		}

		PropertiesData ISettingsModel.GetPropertiesData()
		{
			ThrowIfNoInit();

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
					throw new InvalidOperationException(string.Format(Strings.PropertyIsNotReadable, property.Name));
				}

				var propertyData = SettingsPropertyDataFactory.Create(new ArgsInfo(property.Name, property.PropertyType, model, service));

				propertiesData.Add(property.Name, propertyData);
			}

			return propertiesData;
		}

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
				throw new InvalidOperationException(string.Format(Strings.FailedSetPropertyValue, propertyName, GetType().FullName), ex);
			}
		}

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
				throw new InvalidOperationException(string.Format(Strings.FailedGetPropertyValue, propertyName, GetType().FullName), ex);
			}
		}
	}
}