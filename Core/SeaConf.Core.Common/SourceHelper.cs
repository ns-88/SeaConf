using SeaConf.Common;
using System.Diagnostics.CodeAnalysis;

namespace SeaConf.Core.Common;

internal static class SourceHelper
{
	[DoesNotReturn]
	public static void ThrowCannotConverted(string value, Type type)
	{
		throw new InvalidOperationException(string.Format(Resources.StringValueCannotConvertedToType, type, value));
	}

	[DoesNotReturn]
	public static void ThrowCannotConverted<T>(string value)
	{
		throw new InvalidOperationException(string.Format(Resources.StringValueCannotConvertedToType, typeof(T).Name, value));
	}

	public static T ThrowIfFailedCastType<T>(object value)
	{
		if (value is not T typedValue)
		{
			throw new InvalidCastException(string.Format(Resources.FailedToCastTypeValue, value.GetType(), typeof(T)));
		}

		return typedValue;
	}
}