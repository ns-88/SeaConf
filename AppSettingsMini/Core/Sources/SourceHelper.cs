using System;
using System.Diagnostics.CodeAnalysis;
using AppSettingsMini.Infrastructure;

namespace AppSettingsMini.Core.Sources
{
    internal static class SourceHelper
    {
        [DoesNotReturn]
        public static void ThrowCannotConverted<T>(string value)
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