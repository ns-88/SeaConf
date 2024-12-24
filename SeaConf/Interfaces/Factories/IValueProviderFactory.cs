using System;
using System.Collections;

namespace SeaConf.Interfaces.Factories
{
    /// <summary>
    /// Factory that creates provider for setting and getting data of a specific type.
    /// </summary>
    public interface IValueProviderFactory
    {
        /// <summary>
        /// Supported data type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Comparer.
        /// </summary>
        IEqualityComparer Comparer { get; }

        /// <summary>
        /// Creating.
        /// </summary>
        /// <returns>Value provider</returns>
        IValueProvider Create();
    }
}