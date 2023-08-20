using System;

namespace AppSettingsMini.Interfaces
{
	public interface ISettingsPropertyInfo
	{
		string Name { get; }
		Type Type { get; }
	}

	public interface ISettingsPropertyData : ISettingsPropertyInfo
	{
		bool IsModified { get; }
		ISettingsPropertyData<T> ToTyped<T>();
		ISettingsPropertyData ToTyped(Type type);
		T Get<T>();
		internal void Set(ISettingsPropertyData data);
	}

	public interface ISettingsPropertyData<T> : ISettingsPropertyData
	{
		T Get();
		void Set(T value);
	}
}