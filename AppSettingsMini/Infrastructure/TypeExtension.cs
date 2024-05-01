using System;

namespace AppSettingsMini.Infrastructure
{
	internal static class TypeExtension
	{
		public static bool IsReadOnlyByteMemory(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReadOnlyMemory<byte>);
		}
	}
}