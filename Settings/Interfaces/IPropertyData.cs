namespace Settings.Interfaces
{
	public interface IPropertyData
	{
		string Name { get; }
		Type Type { get; }
		bool IsModified { get; }

		IPropertyData<T> ToTyped<T>();
		internal void Set(IPropertyData data);
	}

	public interface IPropertyData<T> : IPropertyData
	{
		T Get();
		void Set(T value);
	}
}