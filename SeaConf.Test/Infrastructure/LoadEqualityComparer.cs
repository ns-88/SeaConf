using SeaConf.Test.Infrastructure.Models;

namespace SeaConf.Test.Infrastructure
{
	internal class LoadEqualityComparer : IEqualityComparer<IProgramSettings>
	{
		public bool Equals(IProgramSettings? x, IProgramSettings? y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (ReferenceEquals(x, null)) return false;
			if (ReferenceEquals(y, null)) return false;

			return x.StringValue == y.StringValue &&
			       x.IntValue == y.IntValue &&
			       x.LongValue == y.LongValue &&
			       x.UlongValue == y.UlongValue &&
			       x.DoubleValue.Equals(y.DoubleValue) &&
			       x.BoolValue == y.BoolValue &&
			       x.EnumValue == y.EnumValue &&
			       x.BytesValue.Span.SequenceEqual(y.BytesValue.Span);
		}

		public int GetHashCode(IProgramSettings obj)
		{
			var hashCode = new HashCode();
			hashCode.Add(obj.StringValue);
			hashCode.Add(obj.IntValue);
			hashCode.Add(obj.LongValue);
            hashCode.Add(obj.UlongValue);
            hashCode.Add(obj.DoubleValue);
			hashCode.Add(obj.BoolValue);
			hashCode.Add((int)obj.EnumValue);
			hashCode.AddBytes(obj.BytesValue.Span);
			return hashCode.ToHashCode();
		}
	}
}