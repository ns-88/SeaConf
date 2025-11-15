namespace SeaConf.Common.Exceptions;

/// <summary>
/// The exception is thrown when a configuration saving error occurs.
/// </summary>
public class ConfigurationSaveFaultException : Exception
{
	public ConfigurationSaveFaultException(string message)
		: base(message)
	{
	}

	public ConfigurationSaveFaultException(string message, Exception exception)
		: base(message, exception)
	{
	}
}