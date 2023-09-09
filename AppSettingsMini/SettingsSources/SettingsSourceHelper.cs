using AppSettingsMini.Infrastructure;
using System;

namespace AppSettingsMini.SettingsSources
{
	internal static class SettingsSourceHelper
	{
		public static void ThrowIfCannotConverted<T>(string value)
		{
			throw new InvalidOperationException(string.Format(Strings.StringValueCannotConvertedToType, typeof(T).Name, value));
		}

		public static T ThrowIfFailedCastType<T>(object value)
		{
			if (value is not T typedValue)
			{
				throw new InvalidCastException(string.Format(Strings.FailedToCastTypeValue, value.GetType(), typeof(T)));
			}

			return typedValue;
		}
	}
}