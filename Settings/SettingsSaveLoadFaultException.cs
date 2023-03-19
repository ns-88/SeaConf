﻿namespace Settings
{
    internal class SettingsSaveLoadFaultException : Exception
    {
        public SettingsSaveLoadFaultException(string message)
            : base(message)
        {

        }

        public SettingsSaveLoadFaultException(string message, Exception exception)
            : base(message, exception)
        {

        }
    }
}