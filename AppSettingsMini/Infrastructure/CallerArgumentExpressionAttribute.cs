#if NET48
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class CallerArgumentExpressionAttribute : Attribute
	{
		public string ParameterName { get; }

		public CallerArgumentExpressionAttribute(string parameterName)
		{
			ParameterName = parameterName;
		}
	}
}
#endif