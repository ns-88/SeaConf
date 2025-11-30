namespace SeaConf.Common.Exceptions;

/// <summary>
/// The exception is thrown when a configuration loading error occurs.
/// </summary>
public class ConfigurationLoadFaultException : Exception
{
	public ConfigurationLoadFaultException(string message) : base(message)
	{
	}

	public ConfigurationLoadFaultException(string message, Exception exception) : base(message, exception)
	{
	}
}