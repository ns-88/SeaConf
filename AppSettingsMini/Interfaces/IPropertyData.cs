using System;

namespace AppSettingsMini.Interfaces
{
	public interface IPropertyInfo
	{
		string Name { get; }
		Type Type { get; }
	}

	public interface IPropertyData : IPropertyInfo
	{
		bool IsModified { get; }
		IPropertyData<T> ToTyped<T>();
		IPropertyData ToTyped(Type type);
		T Get<T>();
		internal void Set(IPropertyData data);
	}

	public interface IPropertyData<T> : IPropertyData
	{
		T Get();
		void Set(T value);
	}
}