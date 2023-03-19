namespace Settings.Interfaces
{
	public interface ISerializer
	{
		ReadOnlyMemory<byte> Serialize(object value, Type type);
		object Deserialize(ReadOnlyMemory<byte> bytes, Type type);
	}
}