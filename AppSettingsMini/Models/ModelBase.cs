using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AppSettingsMini.Core.Factories;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Core;

namespace AppSettingsMini.Models
{
	using PropertiesData = IReadOnlyDictionary<string, IPropertyData>;

	public class ModelBase : IMemoryModel, IMemoryInitializedModel
	{
		private PropertiesData _storage;
		private bool _isInit;

		public string Name { get; private set; }
		public Type Type { get; private set; }

#nullable disable
		// ReSharper disable once NotNullMemberIsNotInitialized
		protected ModelBase()
		{
		}
#nullable restore

		void IMemoryInitializedModel.Initialize(IModelInfo modelInfo, SettingsServiceBase service)
		{
			_storage = CreatePropertiesData(modelInfo.Type, modelInfo.Model, service);
			_isInit = true;

			Name = modelInfo.Name;
			Type = modelInfo.Type;
		}

		private static PropertiesData CreatePropertiesData(IReflect modelType, IMemoryModel model, SettingsServiceBase service)
		{
			var propertiesData = new Dictionary<string, IPropertyData>();
			var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var property in properties)
			{
				if (!property.CanRead)
				{
					throw new InvalidOperationException(string.Format(Strings.PropertyIsNotReadable, property.Name));
				}

				if (property.GetCustomAttribute<ModelAttribute>() != null)
				{
					continue;
				}

				var propertyData = PropertyDataFactory.Create(new ArgsInfo(property.Name, property.PropertyType, model, service));

				propertiesData.Add(property.Name, propertyData);
			}

			return propertiesData;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ThrowIfNoInit()
		{
			if (!_isInit)
			{
				throw new InvalidOperationException(string.Format(Strings.ModelNotInitialized, GetType().Name));
			}
		}

		IEnumerable<IPropertyData> IMemoryModel.GetModifiedProperties()
		{
			ThrowIfNoInit();

			return _storage.Values.Where(x => x.IsModified);
		}

		IReadOnlyCollection<IPropertyData> IMemoryModel.GetPropertiesData()
		{
			ThrowIfNoInit();
			
			return (IReadOnlyCollection<IPropertyData>)_storage.Values;
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