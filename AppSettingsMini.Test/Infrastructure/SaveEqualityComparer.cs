using System.Globalization;
using System.Net;
using AppSettingsMini.Test.Infrastructure.Models;

namespace AppSettingsMini.Test.Infrastructure
{
	internal class SaveEqualityComparer : IEqualityComparer<object>
	{
		private static DateTime TrimDateTime(DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
		}

		private static T Get<T>(string key, IReadOnlyDictionary<string, object> map)
		{
			return (T)map[key];
		}

		public new bool Equals(object? x, object? y)
		{
			var lhs = (MockProgramSettings?)x!;
			var rhs = (Dictionary<string, Dictionary<string, object>>?)y!;

			var programSettingsMap = rhs[nameof(IProgramSettings)];
			var userSettingsMap = rhs[nameof(IUserSettings)];
			var addressSettingsMap = rhs[nameof(IAddressSettings)];

			return Get<string>(nameof(IProgramSettings.StringValue), programSettingsMap).Equals(lhs.StringValue, StringComparison.Ordinal) &&
				   Get<int>(nameof(IProgramSettings.IntValue), programSettingsMap) == lhs.IntValue &&
				   Get<long>(nameof(IProgramSettings.LongValue), programSettingsMap) == lhs.LongValue &&
				   Get<double>(nameof(IProgramSettings.DoubleValue), programSettingsMap).Equals(lhs.DoubleValue) &&
				   Get<bool>(nameof(IProgramSettings.BoolValue), programSettingsMap) == lhs.BoolValue &&
				   Enum.Parse<Regime>(Get<string>(nameof(IProgramSettings.EnumValue), programSettingsMap)) == lhs.EnumValue &&
				   Get<ReadOnlyMemory<byte>>(nameof(IProgramSettings.BytesValue), programSettingsMap).Span.SequenceEqual(lhs.BytesValue.Span) &&
				   Get<decimal>(nameof(IProgramSettings.DecimalValue), programSettingsMap) == lhs.DecimalValue &&
				   DateTime.Parse(Get<string>(nameof(IProgramSettings.DateTimeValue), programSettingsMap)) == TrimDateTime(lhs.DateTimeValue) &&
				   DateOnly.Parse(Get<string>(nameof(IProgramSettings.DateOnlyValue), programSettingsMap)) == lhs.DateOnlyValue &&
				   TimeOnly.Parse(Get<string>(nameof(IProgramSettings.TimeOnlyValue), programSettingsMap)) == lhs.TimeOnlyValue &&
				   IPEndPoint.Parse(Get<string>(nameof(IProgramSettings.IpEndPointValue), programSettingsMap)).Equals(lhs.IpEndPointValue);
		}

		public int GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}
}