using System;

namespace AppSettingsMini.Interfaces
{
	public interface ISettingsPropertyData
	{
		string Name { get; }
		Type Type { get; }
		bool IsModified { get; }
		ISettingsPropertyData<T> ToTyped<T>();
		internal void Set(ISettingsPropertyData data);
	}

	public interface ISettingsPropertyData<T> : ISettingsPropertyData
	{
		T Get();
		void Set(T value);
	}
}