namespace SeaConf.Core.Common.Infrastructure;

public static class TypeExtensions
{
	public static bool IsReadOnlyByteMemory(this Type type)
	{
		return type == typeof(ReadOnlyMemory<byte>);
	}
}