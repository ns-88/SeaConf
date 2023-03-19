using Settings.Infrastructure;

namespace Settings
{
	public interface IPropertyData
	{
		string Name { get; }
		Type Type { get; }
		bool IsModified { get; }

		static PropertyData<T> GetTyped<T>(IPropertyData rawValue)
		{
			if (rawValue is not PropertyData<T> typedValue)
			{
				throw new InvalidCastException(string.Format(Strings.PropertyTypeNotCorrect, typeof(T), rawValue.Type));
			}

			return typedValue;
		}

		internal void Set(IPropertyData propertyData);
	}

	public class PropertyData<T> : IPropertyData
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

		void IPropertyData.Set(IPropertyData propertyData)
		{
			_value = IPropertyData.GetTyped<T>(propertyData).Get();
		}
	}
}