using Settings.Interfaces;

namespace Settings
{
	public class StandardSerializer : ISerializer
	{
		public ReadOnlyMemory<byte> Serialize(object value, Type type)
		{
			throw new NotImplementedException();
		}

		public object Deserialize(ReadOnlyMemory<byte> bytes, Type type)
		{
			throw new NotImplementedException();
		}
	}
}