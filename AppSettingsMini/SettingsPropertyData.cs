using System;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;

namespace AppSettingsMini
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
				throw new InvalidCastException(string.Format(Strings.PropertyTypeNotCorrect, typeof(T), Type));
			}

			return typedValue;
		}

		void ISettingsPropertyData.Set(ISettingsPropertyData propertyData)
		{
			_value = propertyData.ToTyped<T>().Get();
		}
	}
}