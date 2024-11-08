using System;

namespace SeaConf.Models
{
    /// <summary>
    /// The exception is thrown when a configuration loading or saving error occurs.
    /// </summary>
    public class ConfigurationLoadOrSaveFaultException : Exception
    {
        public ConfigurationLoadOrSaveFaultException(string message)
            : base(message)
        {
            
        }

        public ConfigurationLoadOrSaveFaultException(string message, Exception exception)
            : base(message, exception)
        {

        }
    }
}