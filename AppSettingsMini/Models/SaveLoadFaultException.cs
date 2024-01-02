using System;

namespace AppSettingsMini.Models
{
    internal class SaveLoadFaultException : Exception
    {
        public SaveLoadFaultException(string message)
            : base(message)
        {

        }

        public SaveLoadFaultException(string message, Exception exception)
            : base(message, exception)
        {

        }
    }
}