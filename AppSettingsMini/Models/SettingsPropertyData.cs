using System;
using System.Reflection;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;

namespace AppSettingsMini.Models
{
	public class SettingsPropertyData<T> : ISettingsPropertyData<T>
	{
#nullable disable
		private readonly ISettingsModel _model;
		private readonly SettingsServiceBase _settingsService;
#nullable restore
		private T? _value;

		public string Name { get; }
		public Type Type { get; }
		public bool IsModified { get; private set; }

		// ReSharper disable once UnusedMember.Global
		internal SettingsPropertyData(string name, Type type, ISettingsModel model, SettingsServiceBase service)
		{
			Name = name;
			Type = type;

			_model = model;
			_settingsService = service;
		}

		public SettingsPropertyData(T value, string name)
		{
			_value = value;

			Type = typeof(T);
			Name = name;
		}

		public T Get()
		{
			return _value!;
		}

		public TData Get<TData>()
		{
			if (_value is not TData data)
			{
				throw new InvalidCastException(string.Format(Strings.PropertyTypeNotCorrect, Type, typeof(TData)));
			}

			return data;
		}

		public void Set(T value)
		{
			Guard.ThrowIfNull(value);

			var comparer = _settingsService.ComparersManager.Get<T>();

			if (comparer.Equals(_value!, value))
			{
				return;
			}

			_value = value;

			IsModified = true;

			_settingsService.RaisePropertyChanged(Name, _model);
		}

		ISettingsPropertyData<TData> ISettingsPropertyData.ToTyped<TData>()
		{
			if (this is not ISettingsPropertyData<TData> typedValue)
			{
				throw new InvalidCastException(string.Format(Strings.PropertyTypeNotCorrect, Type, typeof(TData)));
			}

			return typedValue;
		}

		ISettingsPropertyData ISettingsPropertyData.ToTyped(Type type)
		{
			const string methodName = "AppSettingsMini.Interfaces.ISettingsPropertyData.ToTyped";

			var method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);

			if (method == null)
			{
				throw new InvalidOperationException(string.Format(Strings.MethodNotFound, methodName));
			}

			var genericMethod = method.MakeGenericMethod(type);

			return (ISettingsPropertyData)genericMethod.Invoke(this, null)!;
		}

		void ISettingsPropertyData.Set(ISettingsPropertyData propertyData)
		{
			_value = propertyData.ToTyped<T>().Get();
		}
	}
}