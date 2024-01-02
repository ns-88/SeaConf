using System;
using System.Runtime.CompilerServices;

namespace AppSettingsMini.Infrastructure
{
	internal static class Guard
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ThrowIfNull<T>(T source, [CallerArgumentExpression(nameof(source))] string? paramName = null)
		{
			if (source == null)
				throw new ArgumentNullException(paramName);

			return source;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ThrowIfEmptyString(string source, [CallerArgumentExpression(nameof(source))] string? paramName = null)
		{
			if (string.IsNullOrWhiteSpace(source))
				throw new ArgumentException(paramName);

			return source;
		}
	}
}