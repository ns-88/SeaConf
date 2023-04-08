using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;

namespace AppSettingsMini
{
	public class PropertyData<T> : IPropertyData<T>
	{
		private T? _value;

		public string Name { get; }
		public Type Type { get; }
		public bool IsModified { get; private set; }

		public PropertyData(string name, Type type)
		{
			Name = name;
			Type = type;
		}

		public PropertyData(T value, string name)
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
			_value = value;

			IsModified = true;
		}

		IPropertyData<TData> IPropertyData.ToTyped<TData>()
		{
			if (this is not IPropertyData<TData> typedValue)
			{
				throw new InvalidCastException(string.Format(Strings.PropertyTypeNotCorrect, typeof(T), Type));
			}

			return typedValue;
		}

		void IPropertyData.Set(IPropertyData propertyData)
		{
			_value = propertyData.ToTyped<T>().Get();
		}
	}
}